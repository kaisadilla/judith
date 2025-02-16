using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.analyzers;

/// <summary>
/// Traverses all the nodes in a compilation unit, creating all the symbols
/// defined in it.
/// </summary>
public class SymbolTableBuilder : SyntaxVisitor {
    private Compilation _cmp;

    private SymbolTable _currentTable;

    public SymbolTableBuilder (Compilation program) {
        _cmp = program;
        _currentTable = program.SymbolTable;
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    public override void Visit (FunctionDefinition node) {
        string name = node.Identifier.Name;

        var symbol = _currentTable.AddSymbol(SymbolKind.Function, name);
        _cmp.Binder.BindFunctionDefinition(node, symbol);

        if (_currentTable.TryGetInnerTable(name, out var innerTable) == false) {
            throw new Exception(
                $"Inner table '{name}' in " +
                $"'{_currentTable.TableSymbol.FullyQualifiedName}' should exist."
            );
        }

        _currentTable = innerTable;
        Visit(node.Parameters);
        Visit(node.Body);
    }

    public override void Visit (LocalDeclarator node) {
        var symbol = _currentTable.AddSymbol(SymbolKind.Local, node.Identifier.Name);
        _cmp.Binder.BindLocalDeclarator(node, symbol);
    }
}
