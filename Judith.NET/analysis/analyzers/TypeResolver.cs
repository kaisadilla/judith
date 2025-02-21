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

        Visit(node.Body);
        Visit(node.Parameters);
        VisitIfNotNull(node.ReturnTypeAnnotation);

        if (TypeInfo.IsResolved(boundFuncDef.ReturnType)) return;

        // If the return type is explicitly declared.
        if (node.ReturnTypeAnnotation != null) {
            var boundAnnot = Binder.GetBoundNodeOrThrow<BoundTypeAnnotation>(
                node.ReturnTypeAnnotation
            );

            boundFuncDef.Symbol.Type = boundAnnot.Symbol.Type;
            boundFuncDef.ReturnType = boundFuncDef.Symbol.Type;
        }
        // Else, if it's inferred from its body.
        else {
            var boundBody = Binder.GetBoundNodeOrThrow<BoundBlockStatement>(
                node.Body
            );

            boundFuncDef.Symbol.Type = boundBody.Type;
            boundFuncDef.ReturnType = boundFuncDef.Symbol.Type;
        }
    }

    public override void Visit (LocalDeclarationStatement node) {
        if (node.Initializer != null) {
            Visit(node.Initializer);
        }

        _cmp.Binder.BindLocalDeclarationStatement(node);
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

        _cmp.Binder.BindAssignmentExpression(node);
    }

    public override void Visit (BinaryExpression node) {
        Visit(node.Left);
        Visit(node.Right);

        _cmp.Binder.BindBinaryExpression(node);
    }

    public override void Visit (LeftUnaryExpression node) {
        Visit(node.Expression);

        _cmp.Binder.BindLeftUnaryExpression(node);
    }

    public override void Visit (CallExpression node) {
        Visit(node.Callee);
        Visit(node.Arguments);

        _cmp.Binder.BindCallExpression(node);
    }

    // TODO: AccessExpression

    public override void Visit (GroupExpression node) {
        Visit(node.Expression);

        _cmp.Binder.BindGroupExpression(node);
    }

    public override void Visit (IdentifierExpression node) {
        _cmp.Binder.BindIdentifierExpression(node);
    }

    public override void Visit (LiteralExpression node) {
        _cmp.Binder.BindLiteralExpression(node);
    }

    public override void Visit (EqualsValueClause node) {
        Visit(node.Value);
    }
}
