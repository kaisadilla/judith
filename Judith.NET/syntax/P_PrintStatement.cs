using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class P_PrintStatement : Statement {
    public Expression Expression { get; init; }

    public Token? P_PrintToken { get; init; }

    public P_PrintStatement (Expression expression) : base(SyntaxKind.P_PrintStatement) {
        Expression = expression;

        Children.Add(Expression);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return "|__p_print> " + Expression.ToString() + " <|";
    }
}
