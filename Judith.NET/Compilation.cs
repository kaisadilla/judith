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
    public TypeTable TypeTable { get; private set; }

    public Compilation (List<CompilerUnit> units) {
        _units = units;
        SymbolTable = SymbolTable.CreateGlobalTable();
        TypeTable = new();
    }

    public void Analyze () {
        NativeFeatures.AddNativeTypes(TypeTable, SymbolTable);

        SymbolTableBuilder symbolTableBuilder = new(this);
        foreach (var cu in _units) {
            symbolTableBuilder.Analyze(cu);
        }

        //SymbolResolver symbolResolver = new(this);
        //foreach (var cu in _units) {
        //    symbolResolver.Analyze(cu);
        //}
        //
        //TypeTableBuilder typeTableBuilder = new(this);
        //foreach (var cu in _units) {
        //    typeTableBuilder.Analyze(cu);
        //}
        //
        //TypeResolver typeResolver = new(this);
        //foreach (var cu in _units) {
        //    typeResolver.Analyze(cu);
        //}
    }
}
