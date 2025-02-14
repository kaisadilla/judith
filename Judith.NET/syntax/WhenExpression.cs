using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class WhenExpression : Expression {
    public Statement Statement { get; private init; }
    public Expression Test { get; private init; }

    public Token? WhenToken { get; init; }

    public WhenExpression (Statement statement, Expression test)
        : base(SyntaxKind.WhenExpression)
    {
        Statement = statement;
        Test = test;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return Statement.ToString() + " when " + Test.ToString();
    }
}
