using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class ReturnStatement : Statement {
    public Expression? Expression { get; init; }

    public Token? ReturnToken { get; init; }

    public ReturnStatement (Expression? expression) : base(SyntaxKind.ReturnStatement) {
        Expression = expression;

        if (Expression != null) Children.Add(Expression);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
