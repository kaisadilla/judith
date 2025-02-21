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

    private ScopeResolver _scope;

    public SymbolTableBuilder (Compilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp.Binder, _cmp.SymbolTable);
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    public override void Visit (FunctionDefinition node) {
        string name = node.Identifier.Name;

        var symbol = _scope.Current.AddSymbol(SymbolKind.Function, name);
        var scope = _scope.Current.CreateInnerTable(ScopeKind.FunctionBlock, symbol);
        _cmp.Binder.BindFunctionDefinition(node, symbol, scope);

        _scope.BeginScope(scope);
        Visit(node.Parameters);
        Visit(node.Body);
        _scope.EndScope(); // If this fails, something is wrong in CreateAnonymousInnerTable().
    }

    public override void Visit (BlockStatement node) {
        // Block statements do not create symbols, but we bind them in this step anyway.
        _cmp.Binder.BindBlockStatement(node);

        base.Visit(node);
    }

    public override void Visit (IfExpression node) {
        Visit(node.Test);

        var consequentScope = _scope.Current.CreateAnonymousInnerTable(ScopeKind.IfBlock);
        SymbolTable? alternateScope = null;

        if (node.Alternate != null) {
            alternateScope = _scope.Current.CreateAnonymousInnerTable(ScopeKind.ElseBlock);
        }

        _cmp.Binder.BindIfExpression(node, consequentScope, alternateScope);

        _scope.BeginScope(consequentScope);
        Visit(node.Consequent);

        if (node.Alternate != null) {
            _scope.BeginScope(alternateScope!);
            Visit(node.Alternate);
        }

        _scope.EndScope();
    }

    public override void Visit (WhileExpression node) {
        Visit(node.Test);

        var bodyScope = _scope.Current.CreateAnonymousInnerTable(ScopeKind.WhileBlock);
        
        _cmp.Binder.BindWhileExpression(node, bodyScope);

        _scope.BeginScope(bodyScope);
        Visit(node.Body);
        _scope.EndScope();
    }

    public override void Visit (LocalDeclarator node) {
        var symbol = _scope.Current.AddSymbol(SymbolKind.Local, node.Identifier.Name);
        _cmp.Binder.BindLocalDeclarator(node, symbol);
    }
}
