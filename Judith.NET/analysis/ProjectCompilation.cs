using Judith.NET.analysis.analyzers;
using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using Judith.NET.message;

namespace Judith.NET.analysis;

public class ProjectCompilation {
    public MessageContainer Messages { get; private set; } = new();

    /// <summary>
    /// All the compiler units that make up this program.
    /// </summary>
    public List<CompilerUnit> Units { get; private set; } = new();

    public List<ICompilation> Dependencies { get; private set; } = new();
    public TypeTable TypeTable { get; private set; }
    public NativeCompilation Native { get; private set; }
    public SymbolTable SymbolTable { get; private set; }
    public Binder Binder { get; private set; }

    public bool IsValidProgram { get; private set; } = false;

    public ProjectCompilation (List<ICompilation> dependencies, List<CompilerUnit> units) {
        if (dependencies.Count == 0) {
            throw new("At least the native dependency must exist.");
        }
        if (dependencies[0] is not NativeCompilation native) {
            throw new("The first dependency must be the native dependency.");
        }

        Dependencies = dependencies;
        Units = units;
        SymbolTable = SymbolTable.CreateGlobalTable();
        Binder = new(this);
        Native = native;
        TypeTable = new();
    }

    public void Analyze () {
        // Add implicit nodes (such as implicit return statements) to the tree.
        ImplicitNodeAnalyzer implicitNodeAnalyzer = new(this);
        foreach (var cu in Units) {
            implicitNodeAnalyzer.Analyze(cu);
        }
        Messages.Add(implicitNodeAnalyzer.Messages);
        if (Messages.HasErrors) return;

        // TODO: -1. Evaluate wellformedness (semantically agnostic)

        // Add all the different symbols that exist in the program to the table,
        // and binds declarations to the symbols they create.
        SymbolTableBuilder symbolTableBuilder = new(this);
        foreach (var cu in Units) {
            symbolTableBuilder.Analyze(cu);
        }
        if (Messages.HasErrors) return;

        // Resolves which symbol each identifier is referring to.
        SymbolResolver symbolResolver = new(this);
        foreach (var cu in Units) {
            symbolResolver.Analyze(cu);
        }
        Messages.Add(symbolResolver.Messages);
        if (Messages.HasErrors) return;

        ResolveTypes();
        if (Messages.HasErrors) return;
        ResolveTypes();
        if (Messages.HasErrors) return;
        ResolveTypes();
        if (Messages.HasErrors) return;

        TypeAnalyzer typeAnalizer = new(this);
        foreach (var cu in Units) {
            typeAnalizer.Analyze(cu);
        }
        Messages.Add(typeAnalizer.Messages);
        if (Messages.HasErrors) return;

        //Binder.ResolveIncomplete();

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

        BlockTypeResolver blockTypeResolver = new(this);
        foreach (var cu in Units) {
            blockTypeResolver.Analyze(cu);
        }
        Messages.Add(blockTypeResolver.Messages);
    }
}
