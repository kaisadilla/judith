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
        var scope = _currentTable.CreateInnerTable(ScopeKind.FunctionBlock, symbol);
        _cmp.Binder.BindFunctionDefinition(node, symbol, scope);

        _currentTable = scope;
        Visit(node.Parameters);
        Visit(node.Body);
        _currentTable = _currentTable.OuterTable!; // If this is null, something is wrong in CreateAnonymousInnerTable().
    }

    public override void Visit (IfExpression node) {
        Visit(node.Test);

        var consequentScope = _currentTable.CreateAnonymousInnerTable(ScopeKind.IfBlock);
        SymbolTable? alternateScope = null;

        if (node.Alternate != null) {
            alternateScope = _currentTable.CreateAnonymousInnerTable(ScopeKind.ElseBlock);
        }

        _cmp.Binder.BindIfExpression(node, consequentScope, alternateScope);

        _currentTable = consequentScope;
        Visit(node.Consequent);

        if (node.Alternate != null) {
            _currentTable = alternateScope!;
            Visit(node.Alternate);
        }

        _currentTable = _currentTable.OuterTable!;
    }

    public override void Visit (LocalDeclarator node) {
        var symbol = _currentTable.AddSymbol(SymbolKind.Local, node.Identifier.Name);
        _cmp.Binder.BindLocalDeclarator(node, symbol);
    }
}
