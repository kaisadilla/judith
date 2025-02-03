using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class GroupExpression : Expression {
    public Expression Expression { get; init; }
    public Token? LeftParenthesisToken { get; init; }
    public Token? RightParenthesisToken { get; init; }

    public GroupExpression (Expression expression)
        : base(SyntaxKind.GroupExpression)
    {
        Expression = expression;
    }

    public override string ToString () {
        return $"({Expression})";
    }
}
