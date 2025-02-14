using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class AssignmentExpression : Expression {
    public Expression Left { get; init; }
    public Operator Operator { get; init; }
    public Expression Right { get; init; }

    public AssignmentExpression (
        Expression left, Operator op, Expression right
    )
        : base(SyntaxKind.AssignmentExpression)
    {
        Left = left;
        Operator = op;
        Right = right;

        Children.Add(Left, Right);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return $"({Left} = {Right})";
    }
}
