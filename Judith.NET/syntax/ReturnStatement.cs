using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class ReturnStatement : Statement {
    public Expression Expression { get; init; }
    
    public Token? ReturnToken { get; init; }

    public ReturnStatement (Expression expression) : base(SyntaxKind.ReturnStatement) {
        Expression = expression;
    }

    public override string ToString () {
        return "|return> " + Expression.ToString() + " <|";
    }
}
