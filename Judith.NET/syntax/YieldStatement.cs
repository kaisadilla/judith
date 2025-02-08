using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class YieldStatement : Statement {
    public Expression Expression { get; init; }
    
    public Token? YieldToken { get; init; }

    public YieldStatement (Expression expression) : base(SyntaxKind.YieldStatement) {
        Expression = expression;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return "|yield> " + Expression.ToString() + " <|";
    }
}
