using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;
public class BinaryExpression : Expression {
    public Operator Operator { get; init; }
    public Expression Left { get; init; }
    public Expression Right { get; init; }

    public BinaryExpression (Operator op, Expression left, Expression right)
        : base(SyntaxKind.BinaryExpression) {
        Operator = op;
        Left = left;
        Right = right;

        Children.Add(Operator, Left, Right);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
