using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public abstract class UnaryExpression : Expression {
    protected UnaryExpression (SyntaxKind kind) : base(kind) { }
}

public class LeftUnaryExpression : UnaryExpression {
    public Expression Expression { get; init; }
    public Operator Operator { get; init; }

    public LeftUnaryExpression (Operator op, Expression expr)
        : base(SyntaxKind.LeftUnaryExpression)
    {
        Operator = op;
        Expression = expr;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return $"({Operator} {Expression})";
    }
}