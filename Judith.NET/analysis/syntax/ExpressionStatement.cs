using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class ExpressionStatement : Statement {
    public Expression Expression { get; init; }

    public ExpressionStatement (Expression expression)
        : base(SyntaxKind.ExpressionStatement) {
        Expression = expression;

        Children.Add(Expression);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        throw new NotImplementedException();
    }

    public override string ToString () {
        return $"|> {Expression} <|";
    }
}
