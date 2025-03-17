using Judith.NET.analysis.analyzers;
using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using Judith.NET.ir;
using Judith.NET.message;

namespace Judith.NET.analysis;

public class JudithCompilation {
    public string AssemblyName { get; private init; }

    public JudithProgram Program { get; private set; }

    public PseudoTypeCollection PseudoTypes { get; private set; }
    public SymbolTable SymbolTable { get; private set; }
    public Binder Binder { get; private set; }

    public MessageContainer Messages { get; private set; } = new();

    public bool IsValidProgram { get; private set; } = false;

    public JudithCompilation (
        string assemblyName, List<CompilerUnit> units
    ) {
        AssemblyName = assemblyName;

        var noType = TypeSymbol.FreeSymbol(SymbolKind.PseudoType, "<error-type>");

        PseudoTypes = new() {
            NoType = noType,

            Unresolved = TypeSymbol.FreeSymbol(SymbolKind.UnresolvedPseudoType, "!Unresolved"),

            Error = TypeSymbol.FreeSymbol(SymbolKind.ErrorPseudoType, "<error-type>"),

            Anonymous = TypeSymbol.FreeSymbol(SymbolKind.PseudoType, "<anonymous-type>"),
            Function = TypeSymbol.FreeSymbol(SymbolKind.FunctionType, "Function"),
        };

        PseudoTypes.Unresolved.Type = noType;
        PseudoTypes.Error.Type = noType;
        PseudoTypes.Function.Type = noType;

        Program = new() {
            Units = units,
            NativeHeader = new(IRNativeHeader.Ver1(), this),
            Dependencies = [],
        };

        SymbolTable = SymbolTable.CreateGlobalTable(AssemblyName);
        Binder = new(this);
    }

    public void Analyze () {
        // 1. Add implicit nodes.
        ImplicitNodeAnalyzer implicitNodeAnalyzer = new(this);
        foreach (var cu in Program.Units) {
            implicitNodeAnalyzer.Analyze(cu);
        }
        Messages.Add(implicitNodeAnalyzer.Messages);
        if (Messages.HasErrors) return;

        // 2. Evaluate wellformedness (semantically agnostic).
        // TODO.

        // 3. Build symbol table.
        // Add all the different symbols that exist in the program to the table,
        // and binds declarations to the symbols they create.
        SymbolTableBuilder symbolTableBuilder = new(this);
        foreach (var cu in Program.Units) {
            symbolTableBuilder.Analyze(cu);
        }
        if (Messages.HasErrors) return;

        // 4. Resolve symbols.
        // Resolves which symbol each identifier is referring to.
        SymbolResolver symbolResolver = new(this);
        foreach (var cu in Program.Units) {
            symbolResolver.Analyze(cu);
        }
        Messages.Add(symbolResolver.Messages);
        if (Messages.HasErrors) return;

        // 5. Resolve types & 6. Resolve block types.
        ResolveTypes();
        if (Messages.HasErrors) return;

        // 7. Evaluate wellformedness (semantically).
        // 7.1. Type analysis.
        TypeAnalyzer typeAnalizer = new(this);
        foreach (var cu in Program.Units) {
            typeAnalizer.Analyze(cu);
        }
        Messages.Add(typeAnalizer.Messages);
        if (Messages.HasErrors) return;

        Messages.Add(Binder.Messages);

        IsValidProgram = Messages.HasErrors == false;
    }

    private void ResolveTypes () {
        // Resolves any type in the AST that can be resolved. As some nodes
        // reference other nodes (whose type may not be resolved yet), this
        // pass will leave some types unresolved.
        TypeResolver typeResolver = new(this);
        foreach (var cu in Program.Units) {
            typeResolver.Analyze(cu);
        }
        Messages.Add(typeResolver.Messages);

        int passes = 0; // TODO: Temporary.
        while (typeResolver.IsComplete == false) {
            typeResolver.CompleteAnalysis();
            passes++;

            if (passes > 256) throw new("256 passes???");
        }

        BlockTypeResolver blockTypeResolver = new(this);
        foreach (var cu in Program.Units) {
            blockTypeResolver.Analyze(cu);
        }
        Messages.Add(blockTypeResolver.Messages);
    }

    public class PseudoTypeCollection {
        public required TypeSymbol NoType { get; init; }

        public required TypeSymbol Unresolved { get; init; }

        public required TypeSymbol Error { get; init; }

        public required TypeSymbol Anonymous { get; init; }
        public required TypeSymbol Function { get; init; }
    }
}
