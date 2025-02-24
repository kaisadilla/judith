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
/// Traverses all the nodes in a compilation unit, resolving the type of every
/// expression that has a type.
/// </summary>
public class TypeResolver : SyntaxVisitor {
    public MessageContainer Messages { get; private set; } = new();

    private readonly Compilation _cmp;
    private readonly ScopeResolver _scope;

    private Binder Binder => _cmp.Binder;
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
        var boundFuncDef = Binder.GetBoundNodeOrThrow<BoundFunctionDefinition>(node);

        VisitIfNotNull(node.ReturnTypeAnnotation);

        _scope.BeginScope(node);
        Visit(node.Body);
        Visit(node.Parameters);
        _scope.EndScope();

        // If the type 
        if (TypeInfo.IsResolved(boundFuncDef.ReturnType) == false) {
            // If the return type is explicitly declared.
            if (node.ReturnTypeAnnotation != null) {
                TypeInfo type = GetTypeInfo(node.ReturnTypeAnnotation);

                boundFuncDef.Symbol.ReturnType = type;
                boundFuncDef.ReturnType = boundFuncDef.Symbol.ReturnType;
            }
            // Else, if it's inferred from its body.
            else {
                var boundBody = Binder.GetBoundNodeOrThrow<BoundBlockStatement>(
                    node.Body
                );

                boundFuncDef.Symbol.ReturnType = boundBody.Type;
                boundFuncDef.ReturnType = boundFuncDef.Symbol.ReturnType;
            }
        }

        if (boundFuncDef.Symbol.IsResolved() == false) {
            var overload = Binder.GetOverload(node.Parameters);
            boundFuncDef.ParameterTypes = overload;
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
        TypeInfo implicitType = TypeInfo.UnresolvedType;
        if (node.Initializer != null) {
            if (Binder.TryGetBoundNode(node.Initializer.Value, out BoundExpression? expr)) {
                if (TypeInfo.IsResolved(expr.Type)) implicitType = expr.Type;
            }
        }

        TypeInfo inheritedType = implicitType; // Scaffolding.
        foreach (var localDecl in node.DeclaratorList.Declarators) {
            var boundLocalDecl = Binder.GetBoundNodeOrThrow<BoundLocalDeclarator>(localDecl);

            if (TypeInfo.IsResolved(boundLocalDecl.Type) == false) {
                if (localDecl.TypeAnnotation == null) {
                    boundLocalDecl.Symbol.Type = inheritedType;
                }
                else {
                    TypeInfo? type = GetTypeInfo(localDecl.TypeAnnotation);
                    boundLocalDecl.Symbol.Type = type;
                }

                boundLocalDecl.Type = boundLocalDecl.Symbol.Type;
            }

            implicitType = boundLocalDecl.Type ?? TypeInfo.UnresolvedType;
        }
    }

    public override void Visit (ReturnStatement node) {
        var boundNode = _cmp.Binder.BindReturnStatement(node);

        if (TypeInfo.IsResolved(boundNode.Type)) return;

        if (node.Expression == null) {
            boundNode.Type = TypeInfo.VoidType;
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

        if (TypeInfo.IsResolved(boundNode.Type)) return;

        var boundRight = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Right);
        boundNode.Type = boundRight.Type ?? TypeInfo.UnresolvedType;
    }

    public override void Visit (BinaryExpression node) {
        Visit(node.Left);
        Visit(node.Right);

        var boundNode = _cmp.Binder.BindBinaryExpression(node);

        if (TypeInfo.IsResolved(boundNode.Type)) return;

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
                || TypeInfo.IsResolved(boundLeft.Type) == false
                || TypeInfo.IsResolved(boundRight.Type) == false
            ) {
                boundNode.Type = TypeInfo.UnresolvedType;
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

            boundNode.Type = TypeInfo.ErrorType;
        }
    }

    public override void Visit (LeftUnaryExpression node) {
        Visit(node.Expression);

        var boundNode = _cmp.Binder.BindLeftUnaryExpression(node);

        if (TypeInfo.IsResolved(boundNode.Type)) return;

        var boundExpr = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);

        boundNode.Type = boundExpr.Type ?? TypeInfo.UnresolvedType;
    }

    public override void Visit (CallExpression node) {
        Visit(node.Callee);
        Visit(node.Arguments);

        var boundNode = _cmp.Binder.BindCallExpression(node);

        if (TypeInfo.IsResolved(boundNode.Type)) return;

        var overload = GetOverload(node.Arguments);
        foreach (var type in overload) {
            if (TypeInfo.IsResolved(type) == false) return;
        }

        // TODO
        // Callee can resolve to either a function, or an expression that
        // returns a function.
        //if (node.Callee.Kind == SyntaxKind.IdentifierExpression) {
        //
        //}
        //
        //if (_scope.Current.TryFindFunctionSymbolsRecursively()

        // TODO: Operator overloading resolution.

    }

    // TODO: AccessExpression

    public override void Visit (GroupExpression node) {
        Visit(node.Expression);

        var boundNode = _cmp.Binder.BindGroupExpression(node);

        if (TypeInfo.IsResolved(boundNode.Type)) return;

        var boundExpr = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);

        boundNode.Type = boundExpr.Type ?? TypeInfo.UnresolvedType;
    }

    public override void Visit (IdentifierExpression node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundIdentifierExpression>(node);

        if (TypeInfo.IsResolved(boundNode.Type)) return;

        boundNode.Type = boundNode.Symbol.Type ?? TypeInfo.UnresolvedType;
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

        if (TypeInfo.IsResolved(boundParam.Type)) return;

        var boundAnnot = Binder.GetBoundNodeOrThrow<BoundTypeAnnotation>(
            node.Declarator.TypeAnnotation
        );

        boundParam.Symbol.Type = boundAnnot.Symbol.Type;
        boundParam.Type = boundParam.Symbol.Type;
    }

    public override void Visit (MemberField node) {
        var boundNode = Binder.GetBoundNodeOrThrow<BoundMemberField>(node);
        if (TypeInfo.IsResolved(boundNode.Type)) return;

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
    private TypeInfo GetTypeInfo (TypeAnnotation annot) {
        var boundAnnot = Binder.GetBoundNodeOrThrow<BoundTypeAnnotation>(
            annot
        );

        return boundAnnot.Symbol.Type ?? TypeInfo.UnresolvedType;
    }

    private List<TypeInfo> GetOverload (ArgumentList args) {
        List<TypeInfo> overload = [];

        foreach (var arg in args.Arguments) {
            var boundArg = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(
                arg.Expression
            );

            overload.Add(boundArg.Type ?? TypeInfo.UnresolvedType);
        }

        return overload;
    }
}
