﻿using Judith.NET.analysis.binder;
using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Judith.NET.analysis.analyzers;

/// <summary>
/// Traverses all the nodes in a compilation unit, resolving every indentifier
/// to a symbol in the program's symbol table.
/// </summary>
public class SymbolResolver : SyntaxVisitor {
    public MessageContainer Messages { get; private set; } = new();

    private readonly JudithCompilation _cmp;
    private readonly ScopeResolver _scope;
    private readonly SymbolFinder _finder;

    private NodeStateManager _nodeStates = new();

    public SymbolResolver (JudithCompilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp);
        _finder = new(_cmp);
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

        if (_nodeStates.IsComplete(node) == false) {
            _nodeStates.Mark(node, true, _scope.Current, true);
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
            _nodeStates.Mark(node, true, _scope.Current, true);
        }
    }

    public override void Visit (WhileExpression node) {
        Visit(node.Test);

        _scope.BeginScope(node);
        Visit(node.Body);
        _scope.EndScope();

        if (_nodeStates.IsComplete(node) == false) {
            _nodeStates.Mark(node, true, _scope.Current, true);
        }
    }

    public override void Visit (AccessExpression node) {
        if (_nodeStates.IsComplete(node)) return;

        if (node.Receiver == null) {
            throw new NotImplementedException("Implicit 'self' not yet supported.");
        }

        Visit(node.Receiver);

        _nodeStates.Mark(node, true, _scope.Current, true);
    }

    public override void Visit (IdentifierExpression node) {
        if (_nodeStates.IsComplete(node)) return;

        if (node.Name.Kind == SyntaxKind.QualifiedIdentifier) {
            throw new NotImplementedException("Qualified identifiers not yet supported.");
        }

        var simpleName = (SimpleIdentifier)node.Name;

        string name = simpleName.Name;

        var symbol = FindSymbolOrErrorMsg(node, name);
        if (symbol == null) {
            _nodeStates.Mark(node, false, _scope.Current, false);
            return;
        }

        var boundNode = _cmp.Binder.BindIdentifierExpression(node, symbol);

        if (symbol is TypeSymbol typeSymbol) {
            boundNode.Type = _cmp.PseudoTypes.NoType;
            boundNode.AssociatedType = typeSymbol;
        }

        _nodeStates.Mark(node, true, _scope.Current, true);
    }

    public override void Visit (TypeAnnotation node) {
        if (_nodeStates.IsComplete(node)) return;

        if (node.Type is not IdentifierType idType) {
            throw new NotImplementedException("Non-literal types not yet supported.");
        }
        if (idType.Name is not SimpleIdentifier simpleId) {
            throw new NotImplementedException("Qualified identifiers not yet supported.");
        }

        string name = simpleId.Name;

        var symbol = FindSymbolOrErrorMsg(node, name);
        if (symbol == null) {
            _nodeStates.Mark(node, false, _scope.Current, false);
            return;
        }

        if (symbol is TypeSymbol typeSymbol) {
            _cmp.Binder.BindTypeAnnotation(node, typeSymbol);
        }
        else {
            Messages.Add(CompilerMessage.Analyzers.TypeExpectedTR(node));
            _cmp.Binder.BindTypeAnnotation(node, _cmp.PseudoTypes.Error);
        }

        _nodeStates.Mark(node, true, _scope.Current, true);
    }

    private T GetBoundNodeOrThrow<T> (SyntaxNode node) where T : BoundNode {
        if (_cmp.Binder.TryGetBoundNode(node, out T? boundNode) == false) {
            throw new($"Node '{node}' should be bound!");
        }

        return boundNode;
    }

    private Symbol? FindSymbolOrErrorMsg (SyntaxNode node, string name) {
        var candidates = _finder.FindRecursively(name, _scope.Current, []);

        if (candidates.Count == 0) {
            Messages.Add(CompilerMessage.Analyzers.NameDoesNotExist(node, name));
            return null;
        }
        else if (candidates.Count > 1) {
            Messages.Add(CompilerMessage.Analyzers.NameIsAmbiguous(node, name));
            return null;
        }
        else {
            return candidates[0];
        }
    }
}
