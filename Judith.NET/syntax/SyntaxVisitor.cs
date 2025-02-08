using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public abstract class SyntaxVisitor {
    public virtual void DefaultVisit (SyntaxNode? node) { }

    //public virtual void Visit (Expression node) => DefaultVisit(node);
    //public virtual void Visit (Statement node) => DefaultVisit(node);
    //public virtual void Visit (Item node) => DefaultVisit(node);
    public virtual void Visit (Literal node) => DefaultVisit(node);
    public virtual void Visit (Identifier node) => DefaultVisit(node);
    public virtual void Visit (EqualsValueClause node) => DefaultVisit(node);
    public virtual void Visit (Operator node) => DefaultVisit(node);
    public virtual void Visit (MatchCase node) => DefaultVisit(node);
    public virtual void Visit (Parameter node) => DefaultVisit(node);
    public virtual void Visit (ParameterList node) => DefaultVisit(node);
    public virtual void Visit (FieldDeclarator node) => DefaultVisit(node);
    public virtual void Visit (LiteralExpression node) => DefaultVisit(node);
    public virtual void Visit (IdentifierExpression node) => DefaultVisit(node);
    public virtual void Visit (ReturnStatement node) => DefaultVisit(node);
    public virtual void Visit (YieldStatement node) => DefaultVisit(node);
    public virtual void Visit (GroupExpression node) => DefaultVisit(node);
    public virtual void Visit (UnaryExpression node) => DefaultVisit(node);
    public virtual void Visit (BinaryExpression node) => DefaultVisit(node);
    public virtual void Visit (AssignmentExpression node) => DefaultVisit(node);
    public virtual void Visit (FieldDeclarationExpression node) => DefaultVisit(node);
    public virtual void Visit (IfExpression node) => DefaultVisit(node);
    public virtual void Visit (MatchExpression node) => DefaultVisit(node);
    public virtual void Visit (LoopExpression node) => DefaultVisit(node);
    public virtual void Visit (WhileExpression node) => DefaultVisit(node);
    public virtual void Visit (ForeachExpression node) => DefaultVisit(node);
    public virtual void Visit (ExpressionStatement node) => DefaultVisit(node);
    public virtual void Visit (LocalDeclarationStatement node) => DefaultVisit(node);
    public virtual void Visit (BlockStatement node) => DefaultVisit(node);
    public virtual void Visit (ArrowStatement node) => DefaultVisit(node);
    public virtual void Visit (FunctionItem node) => DefaultVisit(node);
}
