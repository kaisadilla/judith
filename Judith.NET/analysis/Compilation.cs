using Judith.NET.analysis.analyzers;
using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using Judith.NET.message;

namespace Judith.NET.analysis;

public class Compilation {
    public string Name { get; private init; }

    public MessageContainer Messages { get; private set; } = new();

    /// <summary>
    /// All the compiler units that make up this program.
    /// </summary>
    public List<CompilerUnit> Units { get; private set; } = new();

    public NativeHeader Native { get; private set; }
    public List<AssemblyHeader> Dependencies { get; private set; } = new();
    public SymbolTable SymbolTable { get; private set; }
    public Binder Binder { get; private set; }

    public bool IsValidProgram { get; private set; } = false;

    public Compilation (
        string name,
        NativeHeader nativeHeader,
        List<AssemblyHeader> dependencies,
        List<CompilerUnit> units
    ) {
        Name = name;

        Native = nativeHeader;
        Dependencies = dependencies;
        Units = units;

        SymbolTable = SymbolTable.CreateGlobalTable(Name);
        Binder = new(this);
    }

    public void Analyze () {
        // 1. Add implicit nodes.
        ImplicitNodeAnalyzer implicitNodeAnalyzer = new(this);
        foreach (var cu in Units) {
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
        foreach (var cu in Units) {
            symbolTableBuilder.Analyze(cu);
        }
        if (Messages.HasErrors) return;

        // 4. Resolve symbols.
        // Resolves which symbol each identifier is referring to.
        SymbolResolver symbolResolver = new(this);
        foreach (var cu in Units) {
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
        foreach (var cu in Units) {
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
        foreach (var cu in Units) {
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
        foreach (var cu in Units) {
            blockTypeResolver.Analyze(cu);
        }
        Messages.Add(blockTypeResolver.Messages);
    }
}
