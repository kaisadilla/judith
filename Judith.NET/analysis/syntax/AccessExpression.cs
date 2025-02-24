using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class AccessExpression : Expression {
    public Expression? Left { get; private init; }
    public Operator Operator { get; private init; }
    public Expression Right { get; private init; }

    public AccessExpression (Expression? leftExpr, Operator op, Expression rightExpr)
        : base(SyntaxKind.AccessExpression) {
        Left = leftExpr;
        Operator = op;
        Right = rightExpr;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
