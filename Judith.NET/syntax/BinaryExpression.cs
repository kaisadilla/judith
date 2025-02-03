using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;
public class BinaryExpression : Expression {
    public Operator Operator { get; init; }
    public Expression Left { get; init; }
    public Expression Right { get; init; }

    public BinaryExpression (Operator op, Expression left, Expression right)
        : base(SyntaxKind.BinaryExpression)
    {
        Operator = op;
        Left = left;
        Right = right;
    }

    public override string ToString () {
        return $"({Left} {Operator} {Right})";
    }
}
