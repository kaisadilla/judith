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

namespace Judith.NET.analysis.analyzers;

/// <summary>
/// Traverses all the nodes in a compilation unit, resolving the type of every
/// expression that has a type.
/// </summary>
public class TypeResolver : SyntaxVisitor {
    public MessageContainer Messages { get; private set; } = new();

    private readonly JudithCompilation _cmp;
    private readonly ScopeResolver _scope;

    private NodeStateManager _nodeStates = new();
    private bool _resolutionMade = false;

    private Binder Binder => _cmp.Binder;

    public bool IsComplete => _nodeStates.AreAllComplete();

    public TypeResolver (JudithCompilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp);
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    public void CompleteAnalysis () {
        var incompleteNodes = _nodeStates.GetIncompleteNodes();

        foreach (var node in incompleteNodes) {
            if (_nodeStates.TryGetScope(node, out var scope) == false) throw new(
                "Incomplete node should have a scope linked to it."
            );

            _scope.BeginScope(scope);
            Visit(node);
            // Do not end scope, we may be in the global scope.
        }
    }

    public override void Visit (FunctionDefinition node) {
        bool isResolved = true;

        var boundNode = Binder.GetBoundNodeOrThrow<BoundFunctionDefinition>(node);

        VisitIfNotNull(node.ReturnTypeAnnotation);

        _scope.BeginScope(node);
        Visit(node.Body);
        Visit(node.Parameters);
        _scope.EndScope();

        if (TypeSymbol.IsResolved(boundNode.Symbol.ReturnType) == false) {
            // If the return type is explicitly declared.
            if (node.ReturnTypeAnnotation != null) {
                var type = GetType(node.ReturnTypeAnnotation);

                boundNode.Symbol.ReturnType = type;
            }
            // Else, it's implicitly "Void".
            else {
                boundNode.Symbol.ReturnType = _cmp.PseudoTypes.Void;
            }

            isResolved = TypeSymbol.IsResolved(boundNode.Symbol.ReturnType);
        }

        if (boundNode.Symbol.AreParamsResolved() == false) {
            // ParamTypes will always be the same size as overload's param types.
            var paramTypes = Binder.GetParamTypes(node.Parameters);

            for (int i = 0; i < paramTypes.Count; i++) {
                boundNode.Symbol.ParamTypes[i] = paramTypes[i];
            }
            
            isResolved = isResolved && boundNode.Symbol.AreParamsResolved();
        }

        _nodeStates.Mark(node, isResolved, _scope.Current);
    }

    public override void Visit (StructTypeDefinition node) {
        var boundNode = Binder.GetBoundNodeOrThrow<BoundStructTypeDefinition>(node);

        _scope.BeginScope(node);
        Visit(node.MemberFields);
        _scope.EndScope();

        _nodeStates.Mark(node, true, _scope.Current);
    }

    public override void Visit (LocalDeclarationStatement node) {
        if (Binder.TryGetBoundNode(
            node, out BoundLocalDeclarationStatement? boundNode) == false
        ) {
            _cmp.Binder.BindLocalDeclarationStatement(node);
        }

        // Visit the initializer first, in case we need its type.
        if (node.Initializer != null) {
            Visit(node.Initializer);
        }

        // TODO: Multiple return values. This is just scaffolding.
        // The initial implicit type is the initializer's type, if able.
        var implicitType = _cmp.PseudoTypes.Unresolved;
        if (node.Initializer != null) {
            if (Binder.TryGetBoundNode(node.Initializer.Value, out BoundExpression? expr)) {
                if (TypeSymbol.IsResolved(expr.Type)) implicitType = expr.Type;
            }
        }

        bool isResolved = true;

        var inheritedType = implicitType; // Scaffolding.
        foreach (var localDecl in node.DeclaratorList.Declarators) {
            var boundLocalDecl = Binder.GetBoundNodeOrThrow<BoundLocalDeclarator>(localDecl);

            if (TypeSymbol.IsResolved(boundLocalDecl.Type) == false) {
                if (localDecl.TypeAnnotation == null) {
                    boundLocalDecl.Symbol.Type = inheritedType;
                }
                else {
                    var type = GetType(localDecl.TypeAnnotation);
                    boundLocalDecl.Symbol.Type = type;
                }

                boundLocalDecl.Type = boundLocalDecl.Symbol.Type;

                isResolved = isResolved && TypeSymbol.IsResolved(boundLocalDecl.Type);
            }

            implicitType = boundLocalDecl.Type ?? _cmp.PseudoTypes.Unresolved;
        }

        _nodeStates.Mark(node, isResolved, _scope.Current);
    }

    public override void Visit (ReturnStatement node) {
        var boundNode = _cmp.Binder.BindReturnStatement(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;

        if (node.Expression == null) {
            boundNode.Type = _cmp.PseudoTypes.Void;
        }
        else {
            Visit(node.Expression);

            var boundExpr = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);

            boundNode.Type = boundExpr.Type;
        }

        bool isResolved = TypeSymbol.IsResolved(boundNode.Type);
        _nodeStates.Mark(node, isResolved, _scope.Current);
    }

    public override void Visit (YieldStatement node) {
        var boundNode = _cmp.Binder.BindYieldStatement(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;

        Visit(node.Expression);

        var boundExpr = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);
        boundNode.Type = boundExpr.Type;

        bool isResolved = TypeSymbol.IsResolved(boundNode.Type);
        _nodeStates.Mark(node, isResolved, _scope.Current);
    }

    public override void Visit (IfExpression node) {
        var boundIfExpr = Binder.GetBoundNodeOrThrow<BoundIfExpression>(node);

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

        _nodeStates.Mark(node, true, _scope.Current);
    }

    public override void Visit (WhileExpression node) {
        Visit(node.Test);

        _scope.BeginScope(node);
        Visit(node.Body);
        _scope.EndScope();

        _nodeStates.Mark(node, true, _scope.Current);
    }

    public override void Visit (AssignmentExpression node) {
        Visit(node.Left);
        Visit(node.Right);

        var boundNode = _cmp.Binder.BindAssignmentExpression(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;

        var boundRight = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Right);
        boundNode.Type = boundRight.Type ?? _cmp.PseudoTypes.Unresolved;

        bool isResolved = TypeSymbol.IsResolved(boundNode.Type);
        _nodeStates.Mark(node, isResolved, _scope.Current);
    }

    public override void Visit (BinaryExpression node) {
        Visit(node.Left);
        Visit(node.Right);

        var boundNode = _cmp.Binder.BindBinaryExpression(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;

        switch (node.Operator.OperatorKind) {
            // Math - their type is determined by the operator function they call,
            // except for natively defined operations (e.g. I64 + I64).
            case OperatorKind.Add:
            case OperatorKind.Subtract:
            case OperatorKind.Multiply:
            case OperatorKind.Divide:
                ResolveMathOperation();
                break;

            // Comparisons - they always return bool.
            case OperatorKind.Equals:
            case OperatorKind.NotEquals:
            case OperatorKind.Like:
            case OperatorKind.ReferenceEquals:
            case OperatorKind.ReferenceNotEquals:
            case OperatorKind.LessThan:
            case OperatorKind.LessThanOrEqualTo:
            case OperatorKind.GreaterThan:
            case OperatorKind.GreaterThanOrEqualTo:
                boundNode.Type = _cmp.Program.NativeHeader.TypeRefs.Bool;
                break;

            // Logical and / or - they return a union of both types.
            case OperatorKind.LogicalAnd:
            case OperatorKind.LogicalOr:
                boundNode.Type = _cmp.Program.NativeHeader.TypeRefs.Bool; // TODO: Scaffolding.
                break;

            // Operators that aren't used in binary expressions:
            case OperatorKind.Assignment:
            case OperatorKind.MemberAccess:
            case OperatorKind.ScopeResolution:
            case OperatorKind.BitwiseNot:
                throw new Exception(
                    $"Invalid operator for a binary expression: " +
                    $"'{node.Operator.OperatorKind}'."
                );
        }

        bool isResolved = TypeSymbol.IsResolved(boundNode.Type);
        _nodeStates.Mark(node, isResolved, _scope.Current);

        void ResolveMathOperation () {
            var opKind = node.Operator.OperatorKind;

            var boundLeft = Binder.GetBoundNodeOrThrow<BoundExpression>(node.Left);
            var boundRight = Binder.GetBoundNodeOrThrow<BoundExpression>(node.Right);

            if (boundLeft == null
                || boundRight == null
                || TypeSymbol.IsResolved(boundLeft.Type) == false
                || TypeSymbol.IsResolved(boundRight.Type) == false
            ) {
                boundNode.Type = _cmp.PseudoTypes.Unresolved;
                return;
            }

            // TODO: Number coalescing.

            // TODO: Implement and check defined operations and stuff.
            if (boundLeft.Type == boundRight.Type) {
                boundNode.Type = boundLeft.Type;
                return;
            }

            boundNode.Type = _cmp.PseudoTypes.Error;
        }
    }

    public override void Visit (LeftUnaryExpression node) {
        Visit(node.Expression);

        var boundNode = Binder.BindUnaryExpression(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;

        var boundExpr = Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);

        boundNode.Type = boundExpr.Type ?? _cmp.PseudoTypes.Unresolved;

        bool isResolved = TypeSymbol.IsResolved(boundNode.Type);
        _nodeStates.Mark(node, isResolved, _scope.Current);
    }

    public override void Visit (ObjectInitializationExpression node) {
        Visit(node.Initializer); // has to be called even when node is resolved.
        VisitIfNotNull(node.Provider);

        var boundNode = Binder.BindObjectInitializationExpression(node);

        if (node.Provider == null) {
            boundNode.Type = _cmp.PseudoTypes.Anonymous;

            _nodeStates.Mark(node, true, _scope.Current);
            return;
        }
        
        var boundProvider = Binder.GetBoundNodeOrThrow<BoundExpression>(node.Provider);

        if (boundProvider is not IBoundIdentifyingExpression boundProvAsId) {
            Messages.Add(CompilerMessage.Analyzers.TypeExpected(node.Provider.Line));
            boundNode.Type = _cmp.PseudoTypes.Error;

            _nodeStates.Mark(node, true, _scope.Current);
            return;
        }

        // If the provider hasn't been resolved, we can't resolve this node either.
        if (TypeSymbol.IsResolved(boundProvAsId.Type) == false) {
            boundNode.Type = _cmp.PseudoTypes.Unresolved;

            _nodeStates.Mark(node, false, _scope.Current);
            return;
        }

        if (boundProvAsId.Type == _cmp.PseudoTypes.NoType) {
            boundNode.Type = boundProvAsId.AssociatedType;
        }
        else {
            boundNode.Type = boundProvAsId.Type;
        }

        bool isResolved = TypeSymbol.IsResolved(boundNode.Type);
        _nodeStates.Mark(node, isResolved, _scope.Current);
    }

    public override void Visit (CallExpression node) {
        Visit(node.Callee);
        Visit(node.Arguments);

        var boundNode = Binder.BindCallExpression(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;
        boundNode.Type = _cmp.PseudoTypes.Unresolved;

        if (node.Callee.Kind == SyntaxKind.IdentifierExpression) {
            var boundCallee = Binder.GetBoundNodeOrThrow<BoundIdentifierExpression>(
                node.Callee
            );

            if (boundCallee.Symbol.Kind == SymbolKind.Function) {
                var funcSymbol = (FunctionSymbol)boundCallee.Symbol;
                boundNode.Type = funcSymbol.ReturnType ?? _cmp.PseudoTypes.Unresolved;
            }
            else {
                throw new NotImplementedException("Cannot call dynamically yet!");
            }
        }

        bool isResolved = TypeSymbol.IsResolved(boundNode.Type);
        _nodeStates.Mark(node, isResolved, _scope.Current);
    }

    public override void Visit (AccessExpression node) {
        if (node.Receiver == null) {
            throw new NotImplementedException("Implicit 'self' not yet supported.");
        }

        Visit(node.Receiver);

        var boundReceiver = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Receiver);

        if (TypeSymbol.IsResolved(boundReceiver.Type) == false) return;

        if (node.AccessKind == AccessKind.Member) {
            if (boundReceiver.Type == _cmp.PseudoTypes.NoType) {
                Messages.Add(CompilerMessage.Analyzers.MemberAccessOnlyOnInstances(node.Line));
                return;
            }

            if (boundReceiver.Type.TryGetMember(node.Member.Name, out MemberSymbol? member) == false) {
                Messages.Add(CompilerMessage.Analyzers.FieldDoesNotExist(
                    boundReceiver.Type.Name, node.Member.Name, node.Member.Line
                ));
                return;
            }

            if (TypeSymbol.IsResolved(member.Type) == false) return;

            var boundNode = _cmp.Binder.BindAccessExpression(node, member);
            boundNode.Type = member.Type;

            bool isResolved = TypeSymbol.IsResolved(boundNode.Type);
            _nodeStates.Mark(node, isResolved, _scope.Current);
        }
        else {
            if (boundReceiver.Type != _cmp.PseudoTypes.NoType) {
                Messages.Add(CompilerMessage.Analyzers.ScopeAccessNotOnInstances(node.Line));
                return;
            }

            throw new NotImplementedException("Scope resolution not yet supported.");
        }
    }

    public override void Visit (GroupExpression node) {
        Visit(node.Expression);

        var boundNode = _cmp.Binder.BindGroupExpression(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;

        var boundExpr = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);

        boundNode.Type = boundExpr.Type ?? _cmp.PseudoTypes.Unresolved;

        bool isResolved = TypeSymbol.IsResolved(boundNode.Type);
        _nodeStates.Mark(node, isResolved, _scope.Current);
    }

    public override void Visit (IdentifierExpression node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundIdentifierExpression>(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;

        if (boundNode.Symbol is TypeSymbol typeSymbol) {
            boundNode.Type = _cmp.PseudoTypes.NoType;
            boundNode.AssociatedType = typeSymbol;
        }
        else {
            boundNode.Type = boundNode.Symbol.Type ?? _cmp.PseudoTypes.Unresolved;
        }

        bool isResolved = TypeSymbol.IsResolved(boundNode.Type);
        _nodeStates.Mark(node, isResolved, _scope.Current);
    }

    public override void Visit (LiteralExpression node) {
        _cmp.Binder.BindLiteralExpression(node);

        // Type calculation is done by the binder.

        _nodeStates.Mark(node, true, _scope.Current);
    }

    public override void Visit (Parameter node) {
        if (node.Declarator.TypeAnnotation == null) {
            throw new(
                "Parameters must have a type annotation (either explicit" +
                "or inherited. Their type cannot be infered."
            );
        }

        Visit(node.Declarator.TypeAnnotation);

        var boundNode = Binder.GetBoundNodeOrThrow<BoundParameter>(node);

        if (TypeSymbol.IsResolved(boundNode.Symbol.Type)) return;

        var boundAnnot = Binder.GetBoundNodeOrThrow<BoundTypeAnnotation>(
            node.Declarator.TypeAnnotation
        );

        boundNode.Symbol.Type = boundAnnot.Type;

        bool isResolved = TypeSymbol.IsResolved(boundNode.Symbol.Type);
        _nodeStates.Mark(node, isResolved, _scope.Current);
    }

    public override void Visit (MemberField node) {
        var boundNode = Binder.GetBoundNodeOrThrow<BoundMemberField>(node);
        if (TypeSymbol.IsResolved(boundNode.Symbol.Type)) return;

        Visit(node.TypeAnnotation);

        var boundAnnot = Binder.GetBoundNodeOrThrow<BoundTypeAnnotation>(
            node.TypeAnnotation
        );

        boundNode.Symbol.Type = boundAnnot.Type;

        bool isResolved = TypeSymbol.IsResolved(boundNode.Symbol.Type);
        _nodeStates.Mark(node, isResolved, _scope.Current);
    }


    private TypeSymbol GetType (TypeAnnotation node) {
        return Binder.GetBoundNodeOrThrow<BoundTypeAnnotation>(node).Type;
    }
}
