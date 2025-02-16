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
    private List<CompilerUnit> _units;

    public SymbolTable SymbolTable { get; private set; }
    public Binder Binder { get; private set; }
    public TypeTable TypeTable { get; private set; }

    public Compilation (List<CompilerUnit> units) {
        _units = units;
        SymbolTable = SymbolTable.CreateGlobalTable();
        Binder = new(this);
        TypeTable = new();
    }

    public void Analyze () {
        NativeFeatures.AddNativeTypes(TypeTable, SymbolTable);

        // Add all the different symbols that exist in the program to the table,
        // and binds declarations to the symbols they create.
        SymbolTableBuilder symbolTableBuilder = new(this);
        foreach (var cu in _units) {
            symbolTableBuilder.Analyze(cu);
        }

        // Resolves which symbol each identifier is referring to.
        SymbolResolver symbolResolver = new(this);
        foreach (var cu in _units) {
            symbolResolver.Analyze(cu);
        }
        Messages.Add(symbolResolver.Messages);

        // Identifies all nodes that define new types and add said types to
        // the type table.
        TypeTableBuilder typeTableBuilder = new(this);
        foreach (var cu in _units) {
            typeTableBuilder.Analyze(cu);
        }

        ResolveTypes();
        ResolveTypes();

        Messages.Add(Binder.Messages);
    }

    private void ResolveTypes () {
        // Resolves any type in the AST that can be resolved. As some nodes
        // reference other nodes (whose type may not be resolved yet), this
        // pass will leave some types unresolved.
        TypeResolver typeResolver = new(this);
        foreach (var cu in _units) {
            typeResolver.Analyze(cu);
        }
        Messages.Add(typeResolver.Messages);
    }
}
