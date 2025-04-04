﻿using System;
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

    public virtual void VisitIfNotNull (SyntaxNode? node) {
        if (node != null) {
            Visit(node);
        }
    }

    //public virtual void Visit (Expression node) => DefaultVisit(node);
    //public virtual void Visit (Statement node) => DefaultVisit(node);
    //public virtual void Visit (Item node) => DefaultVisit(node);

    public virtual void Visit (CompilerUnit node) => VisitChildren(node);
    public virtual void Visit (FunctionDefinition node) => VisitChildren(node);

    public virtual void Visit (AliasTypeDefinition node) => VisitChildren(node);
    public virtual void Visit (StructTypeDefinition node) => VisitChildren(node);

    public virtual void Visit (BlockBody node) => VisitChildren(node);
    public virtual void Visit (ArrowBody node) => VisitChildren(node);
    public virtual void Visit (ExpressionBody node) => VisitChildren(node);

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

    public virtual void Visit (GroupExpression node) => VisitChildren(node);
    public virtual void Visit (AccessExpression node) => VisitChildren(node);
    public virtual void Visit (CallExpression node) => VisitChildren(node);
    public virtual void Visit (ObjectInitializationExpression node) => VisitChildren(node);
    public virtual void Visit (IdentifierExpression node) => VisitChildren(node);
    public virtual void Visit (LiteralExpression node) => VisitChildren(node);

    public virtual void Visit (QualifiedIdentifier node) => VisitChildren(node);
    public virtual void Visit (SimpleIdentifier node) => VisitChildren(node);
    public virtual void Visit (Literal node) => VisitChildren(node);
    public virtual void Visit (LocalDeclaratorList node) => VisitChildren(node);
    public virtual void Visit (LocalDeclarator node) => VisitChildren(node);
    public virtual void Visit (EqualsValueClause node) => VisitChildren(node);
    public virtual void Visit (TypeAnnotation node) => VisitChildren(node);
    public virtual void Visit (Operator node) => VisitChildren(node);
    public virtual void Visit (ParameterList node) => VisitChildren(node);
    public virtual void Visit (Parameter node) => VisitChildren(node);
    public virtual void Visit (ArgumentList node) => VisitChildren(node);
    public virtual void Visit (Argument node) => VisitChildren(node);
    public virtual void Visit (MatchCase node) => VisitChildren(node);

    public virtual void Visit (ObjectInitializer node) => VisitChildren(node);
    public virtual void Visit (FieldInitialization node) => VisitChildren(node);
    public virtual void Visit (MemberField node) => VisitChildren(node);

    public virtual void Visit (GroupType node) => VisitChildren(node);
    public virtual void Visit (IdentifierType node) => VisitChildren(node);
    public virtual void Visit (FunctionType node) => VisitChildren(node);
    public virtual void Visit (TupleArrayType node) => VisitChildren(node);
    public virtual void Visit (RawArrayType node) => VisitChildren(node);
    public virtual void Visit (ObjectType node) => VisitChildren(node);
    public virtual void Visit (LiteralType node) => VisitChildren(node);
    public virtual void Visit (UnionType node) => VisitChildren(node);

    public virtual void Visit (P_PrintStatement node) => VisitChildren(node);
}
