using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class IfExpression : Expression {
    public Expression Test { get; init; }
    public Statement Consequent { get; init; }
    public Statement? Alternate { get; init; }

    public Token? IfToken { get; init; }
    public Token? ElseToken { get; init; }

    public IfExpression (Expression test, Statement consequent, Statement? alternate)
        : base(SyntaxKind.IfExpression) {
        Test = test;
        Consequent = consequent;
        Alternate = alternate;

        Children.Add(Test, Consequent);
        if (Alternate != null) Children.Add(Alternate);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
