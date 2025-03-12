using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public abstract class UnaryExpression : Expression {
    public Operator Operator { get; init; }
    public Expression Expression { get; init; }

    protected UnaryExpression (SyntaxKind kind, Operator op, Expression expr)
        : base(kind)
    {
        Operator = op;
        Expression = expr;
        Children.Add(Operator, Expression);
    }
}

public class LeftUnaryExpression : UnaryExpression {

    public LeftUnaryExpression (Operator op, Expression expr)
        : base(SyntaxKind.LeftUnaryExpression, op, expr) {}

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}