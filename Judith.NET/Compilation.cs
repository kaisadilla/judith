using Judith.NET.analysis;
using Judith.NET.analysis.analyzers;
using Judith.NET.analysis.syntax;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET;

public class Compilation {
    public MessageContainer Messages { get; private set; } = new();

    /// <summary>
    /// All the compiler units that make up this program.
    /// </summary>
    public List<CompilerUnit> Units { get; private set; } = new();

    public NativeFeatures Native { get; private set; }
    public SymbolTable SymbolTable { get; private set; }
    public Binder Binder { get; private set; }
    public TypeTable TypeTable { get; private set; }

    public Compilation (List<CompilerUnit> units) {
        Units = units;
        SymbolTable = SymbolTable.CreateGlobalTable();
        Binder = new(this);
        TypeTable = new();
        Native = new(TypeTable, SymbolTable);
    }

    public void Analyze () {
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

        // Identifies all nodes that define new types and add said types to
        // the type table.
        TypeTableBuilder typeTableBuilder = new(this);
        foreach (var cu in Units) {
            typeTableBuilder.Analyze(cu);
        }
        if (Messages.HasErrors) return;

        ResolveTypes();
        if (Messages.HasErrors) return;
        ResolveTypes();
        if (Messages.HasErrors) return;

        //Binder.ResolveIncomplete();

        Messages.Add(Binder.Messages);
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
    }
}
