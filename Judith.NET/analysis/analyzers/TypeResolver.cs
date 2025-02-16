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

    private Compilation _cmp;

    public TypeResolver (Compilation program) {
        _cmp = program;
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    public override void Visit (FunctionDefinition node) {
        // TODO.
        base.Visit(node);
    }

    public override void Visit (LocalDeclarationStatement node) {
        if (node.Initializer != null) {
            Visit(node.Initializer);
        }

        _cmp.Binder.BindLocalDeclarationStatement(node);
    }

    // if, match, loop, while, foreach

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

    [DoesNotReturn]
    private Exception ExShouldBeBound (string name) {
        throw new Exception($"Identifier '{name}' should be bound.");
    }
}
