using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public abstract class SyntaxVisitor<TResult> {
    /// <summary>
    /// Visits the node given and all of its children.
    /// </summary>
    /// <param name="node">The node to visit.</param>
    protected virtual TResult? Visit (SyntaxNode node) {
        return node.Accept(this);
    }

    /// <summary>
    /// Visits each of the nodes given and all of their children.
    /// </summary>
    /// <param name="nodes">A list of nodes to visit.</param>
    protected virtual void Visit(IEnumerable<SyntaxNode> nodes) {
        foreach (var node in nodes) {
            node.Accept(this);
        }
    }

    /// <summary>
    /// Visits the children of the node given (not the node itself).
    /// </summary>
    /// <param name="node">The node whose children will be visited.</param>
    protected virtual TResult? VisitChildren(SyntaxNode node) {
        foreach (var child in node.Children) {
            child.Accept(this);
        }

        return default;
    }

    //public virtual TResult? Visit (Expression node) => DefaultVisit(node);
    //public virtual TResult? Visit (Statement node) => DefaultVisit(node);
    //public virtual TResult? Visit (Item node) => DefaultVisit(node);

    public virtual TResult? Visit (CompilerUnit node) => VisitChildren(node);
    public virtual TResult? Visit (FunctionDefinition node) => VisitChildren(node);

    public virtual TResult? Visit (BlockStatement node) => VisitChildren(node);
    public virtual TResult? Visit (ArrowStatement node) => VisitChildren(node);
    public virtual TResult? Visit (LocalDeclarationStatement node) => VisitChildren(node);
    public virtual TResult? Visit (ReturnStatement node) => VisitChildren(node);
    public virtual TResult? Visit (YieldStatement node) => VisitChildren(node);

    public virtual TResult? Visit (WhenStatement node) => VisitChildren(node);

    public virtual TResult? Visit (ExpressionStatement node) => VisitChildren(node);

    public virtual TResult? Visit (IfExpression node) => VisitChildren(node);
    public virtual TResult? Visit (MatchExpression node) => VisitChildren(node);
    public virtual TResult? Visit (LoopExpression node) => VisitChildren(node);
    public virtual TResult? Visit (WhileExpression node) => VisitChildren(node);
    public virtual TResult? Visit (ForeachExpression node) => VisitChildren(node);

    public virtual TResult? Visit (AssignmentExpression node) => VisitChildren(node);
    public virtual TResult? Visit (BinaryExpression node) => VisitChildren(node);
    public virtual TResult? Visit (LeftUnaryExpression node) => VisitChildren(node);
    public virtual TResult? Visit (CallExpression node) => VisitChildren(node);
    public virtual TResult? Visit (AccessExpression node) => VisitChildren(node);
    public virtual TResult? Visit (GroupExpression node) => VisitChildren(node);
    public virtual TResult? Visit (IdentifierExpression node) => VisitChildren(node);
    public virtual TResult? Visit (LiteralExpression node) => VisitChildren(node);

    public virtual TResult? Visit (Identifier node) => VisitChildren(node);
    public virtual TResult? Visit (Literal node) => VisitChildren(node);
    public virtual TResult? Visit (LocalDeclaratorList node) => VisitChildren(node);
    public virtual TResult? Visit (LocalDeclarator node) => VisitChildren(node);
    public virtual TResult? Visit (EqualsValueClause node) => VisitChildren(node);
    public virtual TResult? Visit (TypeAnnotation node) => VisitChildren(node);
    public virtual TResult? Visit (Operator node) => VisitChildren(node);
    public virtual TResult? Visit (ParameterList node) => VisitChildren(node);
    public virtual TResult? Visit (Parameter node) => VisitChildren(node);
    public virtual TResult? Visit (ArgumentList node) => VisitChildren(node);
    public virtual TResult? Visit (Argument node) => VisitChildren(node);
    public virtual TResult? Visit (MatchCase node) => VisitChildren(node);

    public virtual TResult? Visit (P_PrintStatement node) => VisitChildren(node);
}
