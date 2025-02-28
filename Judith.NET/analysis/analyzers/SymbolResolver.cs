using Judith.NET.analysis.binder;
using Judith.NET.analysis.semantics;
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

    private NodeStateManager _nodeStates = new();
    public int Resolutions { get; private set; } = 0;

    public SymbolResolver (Compilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp.Binder, _cmp.SymbolTable);
    }

    public void Analyze (CompilerUnit unit) {
        Resolutions = 0;

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

        if (_nodeStates.IsComplete(node) == false) {
            Resolutions++;
            _nodeStates.Completed(node);
        }
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

        if (_nodeStates.IsComplete(node) == false) {
            Resolutions++;
            _nodeStates.Completed(node);
        }
    }

    public override void Visit (WhileExpression node) {
        Visit(node.Test);

        _scope.BeginScope(node);
        Visit(node.Body);
        _scope.EndScope();

        if (_nodeStates.IsComplete(node) == false) {
            Resolutions++;
            _nodeStates.Completed(node);
        }
    }

    public override void Visit (AccessExpression node) {
        if (_nodeStates.IsComplete(node)) return;

        if (node.Receiver == null) {
            throw new NotImplementedException("Implicit 'self' not yet supported.");
        }

        Visit(node.Receiver);
        _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Receiver);

        Resolutions++;
        _nodeStates.Completed(node);
    }

    public override void Visit (IdentifierExpression node) {
        if (_nodeStates.IsComplete(node)) return;

        string name = node.Identifier.Name;

        if (_scope.Current.TryFindSymbolRecursively(name, out Symbol? symbol) == false) {
            Messages.Add(CompilerMessage.Analyzers.NameDoesNotExist(
                name, node.Identifier.Line
            ));
            return;
        }

        var boundNode = _cmp.Binder.BindIdentifierExpression(node, symbol);

        if (symbol is TypeSymbol typeSymbol) {
            boundNode.Type = _cmp.Native.Types.NoType;
            boundNode.AssociatedType = typeSymbol;
        }

        Resolutions++;
        _nodeStates.Completed(node);
    }

    public override void Visit (TypeAnnotation node) {
        if (_nodeStates.IsComplete(node)) return;

        string name = node.Identifier.Name;

        if (_scope.Current.TryFindSymbolRecursively(name, out Symbol? symbol) == false) {
            Messages.Add(CompilerMessage.Analyzers.NameDoesNotExist(
                name, node.Identifier.Line
            ));
            return;
        }

        if (symbol is TypeSymbol typeSymbol) {
            _cmp.Binder.BindTypeAnnotation(node, typeSymbol);
        }
        else {
            Messages.Add(CompilerMessage.Analyzers.TypeExpected(node.Identifier.Line));
            _cmp.Binder.BindTypeAnnotation(node, _cmp.Native.Types.Error);
        }


        Resolutions++;
        _nodeStates.Completed(node);
    }

    private T GetBoundNodeOrThrow<T> (SyntaxNode node) where T : BoundNode {
        if (_cmp.Binder.TryGetBoundNode(node, out T? boundNode) == false) {
            throw new($"Node '{node}' should be bound!");
        }

        return boundNode;
    }
}
