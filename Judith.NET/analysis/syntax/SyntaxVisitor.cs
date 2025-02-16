using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public abstract class SyntaxVisitor {
    /// <summary>
    /// Visits the node given and all of its children.
    /// </summary>
    /// <param name="node">The node to visit.</param>
    protected virtual void Visit (SyntaxNode node) {
        node.Accept(this);
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
    protected virtual void VisitChildren(SyntaxNode node) {
        foreach (var child in node.Children) {
            child.Accept(this);
        }
    }

    //public virtual void Visit (Expression node) => DefaultVisit(node);
    //public virtual void Visit (Statement node) => DefaultVisit(node);
    //public virtual void Visit (Item node) => DefaultVisit(node);

    public virtual void Visit (CompilerUnit node) => VisitChildren(node);
    public virtual void Visit (FunctionDefinition node) => VisitChildren(node);

    public virtual void Visit (BlockStatement node) => VisitChildren(node);
    public virtual void Visit (ArrowStatement node) => VisitChildren(node);
    public virtual void Visit (LocalDeclarationStatement node) => VisitChildren(node);
    public virtual void Visit (ReturnStatement node) => VisitChildren(node);
    public virtual void Visit (YieldStatement node) => VisitChildren(node);

    public virtual void Visit (WhenStatement node) => VisitChildren(node);

    public virtual void Visit (ExpressionStatement node) => VisitChildren(node);

    public virtual void Visit (IfExpression node) => VisitChildren(node);
    public virtual void Visit (MatchExpression node) => VisitChildren(node);
    public virtual void Visit (LoopExpression node) => VisitChildren(node);
    public virtual void Visit (WhileExpression node) => VisitChildren(node);
    public virtual void Visit (ForeachExpression node) => VisitChildren(node);

    public virtual void Visit (AssignmentExpression node) => VisitChildren(node);
    public virtual void Visit (BinaryExpression node) => VisitChildren(node);
    public virtual void Visit (LeftUnaryExpression node) => VisitChildren(node);
    public virtual void Visit (AccessExpression node) => VisitChildren(node);
    public virtual void Visit (GroupExpression node) => VisitChildren(node);
    public virtual void Visit (IdentifierExpression node) => VisitChildren(node);
    public virtual void Visit (LiteralExpression node) => VisitChildren(node);

    public virtual void Visit (Identifier node) => VisitChildren(node);
    public virtual void Visit (Literal node) => VisitChildren(node);
    public virtual void Visit (LocalDeclaratorList node) => VisitChildren(node);
    public virtual void Visit (LocalDeclarator node) => VisitChildren(node);
    public virtual void Visit (EqualsValueClause node) => VisitChildren(node);
    public virtual void Visit (TypeAnnotation node) => VisitChildren(node);
    public virtual void Visit (Operator node) => VisitChildren(node);
    public virtual void Visit (ParameterList node) => VisitChildren(node);
    public virtual void Visit (Parameter node) => VisitChildren(node);
    public virtual void Visit (MatchCase node) => VisitChildren(node);

    public virtual void Visit (P_PrintStatement node) => VisitChildren(node);
}
