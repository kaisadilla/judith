using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public abstract class UnaryExpression : Expression {
    protected UnaryExpression (SyntaxKind kind) : base(kind) { }
}

public class LeftUnaryExpression : UnaryExpression {
    public Operator Operator { get; init; }
    public Expression Expression { get; init; }

    public LeftUnaryExpression (Operator op, Expression expr)
        : base(SyntaxKind.LeftUnaryExpression) {
        Operator = op;
        Expression = expr;

        Children.Add(Operator, Expression);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}