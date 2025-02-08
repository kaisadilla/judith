using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class ExpressionStatement : Statement {
    public Expression Expression { get; init; }

    public ExpressionStatement (Expression expression)
        : base(SyntaxKind.ExpressionStatement)
    {
        Expression = expression;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return $"|> {Expression} <|";
    }
}
