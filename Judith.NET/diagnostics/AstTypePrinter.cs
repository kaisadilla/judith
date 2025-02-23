using Judith.NET.analysis;
using Judith.NET.analysis.binder;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.diagnostics;

public class AstTypePrinter : SyntaxVisitor {
    public List<string> TypedNodes { get; private set; } = new();

    private Compilation _cmp;

    public AstTypePrinter (Compilation cmp) {
        _cmp = cmp;
    }

    public void Analyze () {
        foreach (var u in _cmp.Units) {
            Visit(u);
        }
    }

    public override void Visit (FunctionDefinition node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundFunctionDefinition>(node);

        TypedNodes.Add(
            $"\nFuncDef: {node.Identifier.Name} ({node.Parameters.Parameters.Count}) " +
            $"- Type: {FQN(boundNode.ReturnType)}"
        );

        base.Visit(node);
    }

    public override void Visit (StructTypeDefinition node) {
        // TODO: Bound node

        TypedNodes.Add($"TODO: StructTypeDefinition {node.Identifier.Name}");
        base.Visit(node);
    }

    public override void Visit (BlockStatement node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundBlockStatement>(node);

        TypedNodes.Add($"BlockStmt: Type: {FQN(boundNode.Type)}");
        base.Visit(node);
    }

    public override void Visit (ReturnStatement node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundReturnStatement>(node);

        TypedNodes.Add($"ReturnStmt: ({node.Expression ?? null}) - Type: {FQN(boundNode.Type)}");
    }
    
    public override void Visit (YieldStatement node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundYieldStatement>(node);

        TypedNodes.Add($"YieldStmt: ({node.Expression}) - Type: {FQN(boundNode.Type)}");
    }

    public override void Visit (AssignmentExpression node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundAssignmentExpression>(node);

        TypedNodes.Add($"AssignmentExpr: {node} - Type: {FQN(boundNode.Type)}");
    }

    public override void Visit (BinaryExpression node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundBinaryExpression>(node);

        TypedNodes.Add($"BinaryExpr: {node} - Type: {FQN(boundNode.Type)}");
    }

    public override void Visit (LeftUnaryExpression node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundLeftUnaryExpression>(node);

        TypedNodes.Add($"LeftUnaryExpr: {node} - Type: {FQN(boundNode.Type)}");
    }

    public override void Visit (CallExpression node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundCallExpression>(node);

        TypedNodes.Add($"CallExpr: {node} - Type: {FQN(boundNode.Type)}");
    }

    // TODO: AccessExpression

    public override void Visit (GroupExpression node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundGroupExpression>(node);

        TypedNodes.Add($"GroupExpr: {node} - Type: {FQN(boundNode.Type)}");
    }

    public override void Visit (IdentifierExpression node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundIdentifierExpression>(node);

        TypedNodes.Add($"IdentifierExpr: {node.Identifier.Name} - Type: {FQN(boundNode.Type)}");
    }

    public override void Visit (LiteralExpression node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundLiteralExpression>(node);

        TypedNodes.Add($"LiteralExpr: '{node.Literal.Source}' - Type: {FQN(boundNode.Type)}");
    }

    public override void Visit (LocalDeclarator node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundLocalDeclarator>(node);

        TypedNodes.Add($"LocalDecl: {node.Identifier.Name} - Type: {FQN(boundNode.Type)}");
    }

    public override void Visit (Parameter node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundParameter>(node);

        TypedNodes.Add($"Parameter: {node.Declarator.Identifier.Name} - Type: {FQN(boundNode.Type)}");
    }

    public override void Visit (MemberField node) {
        // TODO: Bound node

        TypedNodes.Add($"TODO: MemberField: {node.Identifier.Name}");
        base.Visit(node);
    }

    private string FQN (TypeInfo? typeInfo) {
        return typeInfo?.FullyQualifiedName ?? "null";
    }
}
