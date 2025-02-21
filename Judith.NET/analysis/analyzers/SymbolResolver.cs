using Judith.NET.analysis.binder;
using Judith.NET.analysis.syntax;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.analyzers;

/// <summary>
/// Traverses all the nodes in a compilation unit, resolving every indentifier
/// to a symbol in the program's symbol table.
/// </summary>
public class SymbolResolver : SyntaxVisitor {
    public MessageContainer Messages { get; private set; } = new();

    private readonly Compilation _cmp;
    private readonly ScopeResolver _scope;

    public SymbolResolver (Compilation cmp) {
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
        _scope.BeginScope(node);

        Visit(node.Parameters);
        Visit(node.Body);

        _scope.EndScope();

        if (node.ReturnTypeAnnotation != null) Visit(node.ReturnTypeAnnotation);
    }

    public override void Visit (IfExpression node) {
        var boundIfExpr = GetBoundNodeOrThrow<BoundIfExpression>(node);

        Visit(node.Test);

        _scope.BeginScope(boundIfExpr.ConsequentScope);
        Visit(node.Consequent);

        if (node.Alternate != null) {
            if (boundIfExpr.AlternateScope == null) throw new Exception(
                "AlternateScope shouldn't be null."
            );

            _scope.BeginScope(boundIfExpr.AlternateScope);
            Visit(node.Alternate);
        }

        _scope.EndScope();
    }

    public override void Visit (WhileExpression node) {
        Visit(node.Test);

        _scope.BeginScope(node);
        Visit(node.Body);
        _scope.EndScope();
    }

    public override void Visit (IdentifierExpression node) {
        string name = node.Identifier.Name;

        if (_scope.Current.TryFindSymbolRecursively(name, out Symbol? symbol) == false) {
            Messages.Add(CompilerMessage.Analyzers.NameDoesNotExist(
                name, node.Identifier.Line
            ));
            return;
        }

        _cmp.Binder.BindIdentifierExpression(node, symbol);
    }

    public override void Visit (TypeAnnotation node) {
        string name = node.Identifier.Name;

        if (_scope.Current.TryFindSymbolRecursively(name, out Symbol? symbol) == false) {
            Messages.Add(CompilerMessage.Analyzers.NameDoesNotExist(
                name, node.Identifier.Line
            ));
            return;
        }

        _cmp.Binder.BindTypeAnnotation(node, symbol);
    }

    private T GetBoundNodeOrThrow<T> (SyntaxNode node) where T : BoundNode {
        if (_cmp.Binder.TryGetBoundNode(node, out T? boundNode) == false) {
            throw new($"Node '{node}' should be bound!");
        }

        return boundNode;
    }
}
