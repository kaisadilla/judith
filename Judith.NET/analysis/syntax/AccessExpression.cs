using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class AccessExpression : Expression {
    public Expression LeftExpr { get; private init; }
    public Operator Operator { get; private init; }
    public Expression RightExpr { get; private init; }

    public AccessExpression (Expression leftExpr, Operator op, Expression rightExpr)
        : base(SyntaxKind.AccessExpression) {
        LeftExpr = leftExpr;
        Operator = op;
        RightExpr = rightExpr;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        throw new NotImplementedException();
    }

    public override string ToString () {
        return "(" + LeftExpr.ToString()
            + (Operator.OperatorKind == OperatorKind.ScopeResolution ? "::" : ".")
            + RightExpr.ToString() + ")";
    }
}
