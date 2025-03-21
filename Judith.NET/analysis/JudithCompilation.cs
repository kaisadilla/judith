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
    public SymbolTable RootSymbolTable { get; private set; }
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

            Auto = TypeSymbol.FreeSymbol(SymbolKind.PseudoType, "Auto"),
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

        RootSymbolTable = SymbolTable.CreateRootTable(AssemblyName, [""]); // TODO: root module is a parameter.
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
        ResolveSymbols();
        if (Messages.HasErrors) return;

        // 5. Resolve types and block types.
        ResolveTypes();
        if (Messages.HasErrors) return;

        // 6. Evaluate wellformedness (semantically).
        // 6.1. Type analysis.
        TypeAnalyzer typeAnalizer = new(this);
        foreach (var cu in Program.Units) {
            typeAnalizer.Analyze(cu);
        }
        Messages.Add(typeAnalizer.Messages);
        if (Messages.HasErrors) return;

        Messages.Add(Binder.Messages);

        IsValidProgram = Messages.HasErrors == false;
    }

    private void ResolveSymbols () {
        SymbolResolver symbolResolver = new(this);
        foreach (var cu in Program.Units) {
            symbolResolver.Analyze(cu);
        }
        Messages.Add(symbolResolver.Messages);
    }

    private void ResolveTypes () {
        TypeResolver typeResolver = new(this);
        BodyTypeResolver bodyTypeResolver = new(this);

        // We do a first pass through all the nodes in the AST. Some of them
        // will have their type resolved immediately, but some will depend on
        // other nodes and thus need to wait for these other nodes to be resolved.
        foreach (var cu in Program.Units) {
            typeResolver.StartAnalysis(cu);
        }
        Messages.Add(typeResolver.Messages);

        // We also calculate the value returned / yielded by bodies. As above,
        // we may not be able to resolve all bodies in the first pass.
        foreach (var cu in Program.Units) {
            bodyTypeResolver.StartAnalysis(cu);
        }
        Messages.Add(bodyTypeResolver.Messages);

#if DEBUG
        int passes = 0;
#endif
        // We now keep iterating over and over
        while (
            typeResolver.NodeStates.AreAllComplete() == false
            || bodyTypeResolver.NodeStates.AreAllComplete() == false
        ) {
            typeResolver.ContinueAnalysis();
            bodyTypeResolver.ContinueAnalysis();

            // If nothing was resolved, then we'll start the next loop from the
            // same state, which will yield the same result (assuming correct
            // implementation).
            if (
                typeResolver.NodeStates.ResolutionMade == false
                && bodyTypeResolver.NodeStates.ResolutionMade == false
            ) {
                throw new(
                    "Type resolution entered an infinite loop, or work done was " +
                    "not properly reported."
                );
            }

#if DEBUG
            passes++;
            if (passes > 1_024) throw new("1024 passes???");
#endif
        }
    }

    public class PseudoTypeCollection {
        public required TypeSymbol NoType { get; init; }

        public required TypeSymbol Unresolved { get; init; }

        public required TypeSymbol Error { get; init; }

        public required TypeSymbol Auto { get; init; }
        public required TypeSymbol Anonymous { get; init; }
        public required TypeSymbol Function { get; init; }
    }
}
