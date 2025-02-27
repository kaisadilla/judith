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
/// Traverses all the nodes in a compilation unit, resolving the type of every
/// expression that has a type.
/// </summary>
public class TypeResolver : SyntaxVisitor {
    public MessageContainer Messages { get; private set; } = new();

    private readonly Compilation _cmp;
    private readonly ScopeResolver _scope;

    private Binder Binder => _cmp.Binder;
    private NativeFeatures.TypeCollection NativeTypes => _cmp.Native.Types;

    public TypeResolver (Compilation cmp) {
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
        var boundNode = Binder.GetBoundNodeOrThrow<BoundFunctionDefinition>(node);

        VisitIfNotNull(node.ReturnTypeAnnotation);

        _scope.BeginScope(node);
        Visit(node.Body);
        Visit(node.Parameters);
        _scope.EndScope();

        // If the type 
        if (TypeSymbol.IsResolved(boundNode.Overload.ReturnType) == false) {
            // If the return type is explicitly declared.
            if (node.ReturnTypeAnnotation != null) {
                var type = GetTypeSymbol(node.ReturnTypeAnnotation);

                boundNode.Overload.ReturnType = type;
            }
            // Else, if it's inferred from its body.
            else {
                var boundBody = Binder.GetBoundNodeOrThrow<BoundBlockStatement>(
                    node.Body
                );

                boundNode.Overload.ReturnType = boundBody.Type;
            }
        }

        if (boundNode.Overload.IsResolved() == false) {
            // ParamTypes will always be the same size as overload's param types.
            var paramTypes = Binder.GetParamTypes(node.Parameters);

            for (int i = 0; i < paramTypes.Count; i++) {
                boundNode.Overload.ParamTypes[i] = paramTypes[i];
            }
        }
    }

    public override void Visit (StructTypeDefinition node) {
        var boundNode = Binder.GetBoundNodeOrThrow<BoundStructTypeDefinition>(node);

        _scope.BeginScope(node);
        Visit(node.MemberFields);
        _scope.EndScope();
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
        var implicitType = NativeTypes.Unresolved;
        if (node.Initializer != null) {
            if (Binder.TryGetBoundNode(node.Initializer.Value, out BoundExpression? expr)) {
                if (TypeSymbol.IsResolved(expr.Type)) implicitType = expr.Type;
            }
        }

        var inheritedType = implicitType; // Scaffolding.
        foreach (var localDecl in node.DeclaratorList.Declarators) {
            var boundLocalDecl = Binder.GetBoundNodeOrThrow<BoundLocalDeclarator>(localDecl);

            if (TypeSymbol.IsResolved(boundLocalDecl.Type) == false) {
                if (localDecl.TypeAnnotation == null) {
                    boundLocalDecl.Symbol.Type = inheritedType;
                }
                else {
                    var type = GetTypeSymbol(localDecl.TypeAnnotation);
                    boundLocalDecl.Symbol.Type = type;
                }

                boundLocalDecl.Type = boundLocalDecl.Symbol.Type;
            }

            implicitType = boundLocalDecl.Type ?? NativeTypes.Unresolved;
        }
    }

    public override void Visit (ReturnStatement node) {
        var boundNode = _cmp.Binder.BindReturnStatement(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;

        if (node.Expression == null) {
            boundNode.Type = NativeTypes.Void;
        }
        else {
            Visit(node.Expression);

            var boundExpr = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);

            boundNode.Type = boundExpr.Type;
        }
    }

    public override void Visit (YieldStatement node) {
        var boundNode = _cmp.Binder.BindYieldStatement(node);

        Visit(node.Expression);

        var boundExpr = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);
        boundNode.Type = boundExpr.Type;
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
    }

    public override void Visit (WhileExpression node) {
        Visit(node.Test);

        _scope.BeginScope(node);
        Visit(node.Body);
        _scope.EndScope();
    }

    public override void Visit (AssignmentExpression node) {
        Visit(node.Left);
        Visit(node.Right);

        var boundNode = _cmp.Binder.BindAssignmentExpression(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;

        var boundRight = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Right);
        boundNode.Type = boundRight.Type ?? NativeTypes.Unresolved;
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
                return;

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
                boundNode.Type = _cmp.Native.Types.Bool;
                return;

            // Logical and / or - they return a union of both types.
            case OperatorKind.LogicalAnd:
            case OperatorKind.LogicalOr:
                boundNode.Type = _cmp.Native.Types.Bool; // TODO: Scaffolding.
                return;

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

        void ResolveMathOperation () {
            var opKind = node.Operator.OperatorKind;

            var boundLeft = Binder.GetBoundNodeOrThrow<BoundExpression>(node.Left);
            var boundRight = Binder.GetBoundNodeOrThrow<BoundExpression>(node.Right);

            if (boundLeft == null
                || boundRight == null
                || TypeSymbol.IsResolved(boundLeft.Type) == false
                || TypeSymbol.IsResolved(boundRight.Type) == false
            ) {
                boundNode.Type = NativeTypes.Unresolved;
                return;
            }

            if (
                _cmp.Native.IsNumericType(boundLeft.Type)
                && _cmp.Native.IsNumericType(boundRight.Type)
            ) {
                boundNode.Type = _cmp.Native.CoalesceNumericTypes(
                    boundLeft.Type, boundRight.Type
                );
                return;
            }

            // TODO: Implement and check defined operations and stuff.
            if (boundLeft.Type == boundRight.Type) {
                boundNode.Type = boundLeft.Type;
                return;
            }

            boundNode.Type = NativeTypes.Error;
        }
    }

    public override void Visit (LeftUnaryExpression node) {
        Visit(node.Expression);

        var boundNode = Binder.BindLeftUnaryExpression(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;

        var boundExpr = Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);

        boundNode.Type = boundExpr.Type ?? NativeTypes.Unresolved;
    }

    public override void Visit (ObjectInitializationExpression node) {
        Visit(node.Initializer); // has to be called even when node is resolved.
        VisitIfNotNull(node.Provider);

        var boundNode = Binder.BindObjectInitializationExpression(node);

        if (node.Provider == null) {
            boundNode.Type = NativeTypes.Anonymous;
            return;
        }
        
        var boundProvider = Binder.GetBoundNodeOrThrow<BoundExpression>(node.Provider);

        // If the provider hasn't been resolved, we can't resolve this node either.
        if (TypeSymbol.IsResolved(boundProvider.Type) == false) {
            boundProvider.Type = NativeTypes.Unresolved;
            return;
        }

        Symbol symbol;
        if (node.Provider.Kind == SyntaxKind.IdentifierExpression) {
            symbol = ((BoundIdentifierExpression)boundProvider).Symbol;
        }
        else if (node.Provider.Kind == SyntaxKind.AccessExpression) {
            //symbol = ((BoundAccessExpression)boundProvider).Symbol;
            throw new NotImplementedException("Access not implemented.");
        }
        else {
            throw new NotImplementedException("This can't happen.");
        }

        if (symbol.Kind == SymbolKind.StructType) {
            boundNode.Type = ((TypeSymbol)symbol).Type;
        }
        else {
            Messages.Add(CompilerMessage.Analyzers.InvalidTypeForObjectInitialization(
                symbol.FullyQualifiedName, node.Provider.Line
            ));
        }
    }

    public override void Visit (CallExpression node) {
        Visit(node.Callee);
        Visit(node.Arguments);

        var boundNode = Binder.BindCallExpression(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;
        boundNode.Type = NativeTypes.Unresolved;

        var paramTypes = GetOverload(node.Arguments);
        foreach (var type in paramTypes) {
            if (TypeSymbol.IsResolved(type) == false) return;
        }

        if (node.Callee.Kind == SyntaxKind.IdentifierExpression) {
            var boundCallee = Binder.GetBoundNodeOrThrow<BoundIdentifierExpression>(
                node.Callee
            );

            if (boundCallee.Symbol.Kind == SymbolKind.Function) {
                var funcSymbol = (FunctionSymbol)boundCallee.Symbol;

                if (funcSymbol.TryGetOverload(paramTypes, out var funcOverload)) {
                    boundNode.Type = funcOverload.ReturnType ?? NativeTypes.Unresolved;
                }
            }
            else {
                throw new NotImplementedException("Cannot call dynamically yet!");
            }
        }
    }

    // TODO: AccessExpression

    public override void Visit (GroupExpression node) {
        Visit(node.Expression);

        var boundNode = _cmp.Binder.BindGroupExpression(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;

        var boundExpr = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);

        boundNode.Type = boundExpr.Type ?? NativeTypes.Unresolved;
    }

    public override void Visit (IdentifierExpression node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundIdentifierExpression>(node);

        if (TypeSymbol.IsResolved(boundNode.Type)) return;

        boundNode.Type = boundNode.Symbol.Type ?? NativeTypes.Unresolved;
    }

    public override void Visit (LiteralExpression node) {
        _cmp.Binder.BindLiteralExpression(node);

        // Type calculation is done by the binder.
    }

    public override void Visit (EqualsValueClause node) {
        Visit(node.Value);
    }

    public override void Visit (Parameter node) {
        if (node.Declarator.TypeAnnotation == null) {
            throw new(
                "Parameters must have a type annotation (either explicit" +
                "or inherited. Their type cannot be infered."
            );
        }

        Visit(node.Declarator.TypeAnnotation);

        var boundParam = Binder.GetBoundNodeOrThrow<BoundParameter>(node);

        if (TypeSymbol.IsResolved(boundParam.Type)) return;

        var boundAnnot = Binder.GetBoundNodeOrThrow<BoundTypeAnnotation>(
            node.Declarator.TypeAnnotation
        );

        boundParam.Symbol.Type = boundAnnot.Symbol.Type;
        boundParam.Type = boundParam.Symbol.Type;
    }

    public override void Visit (MemberField node) {
        var boundNode = Binder.GetBoundNodeOrThrow<BoundMemberField>(node);
        if (TypeSymbol.IsResolved(boundNode.Type)) return;

        Visit(node.TypeAnnotation);

        var boundAnnot = Binder.GetBoundNodeOrThrow<BoundTypeAnnotation>(
            node.TypeAnnotation
        );

        boundNode.Symbol.Type = boundAnnot.Symbol.Type;
        boundNode.Type = boundNode.Symbol.Type;
    }




    /// <summary>
    /// Given a type annotation, returns the type info it refers to. Throws if
    /// the type annotation is not bound.
    /// </summary>
    /// <param name="annot">The annotation to get the type from.</param>
    private TypeSymbol GetTypeSymbol (TypeAnnotation annot) {
        var boundAnnot = Binder.GetBoundNodeOrThrow<BoundTypeAnnotation>(
            annot
        );

        return boundAnnot.Symbol.Type ?? NativeTypes.Unresolved;
    }

    private List<TypeSymbol> GetOverload (ArgumentList args) {
        List<TypeSymbol> overload = [];

        foreach (var arg in args.Arguments) {
            var boundArg = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(
                arg.Expression
            );

            overload.Add(boundArg.Type ?? NativeTypes.Unresolved);
        }

        return overload;
    }
}
