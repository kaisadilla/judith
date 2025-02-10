﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class IfExpression : Expression {
    public Expression Test { get; init; }
    public Statement Consequent { get; init; }
    public Statement? Alternate { get; init; }

    public Token? IfToken { get; init; }
    public Token? ElseToken { get; init; }

    public IfExpression (Expression test, Statement consequent, Statement? alternate)
        : base(SyntaxKind.IfExpression)
    {
        Test = test;
        Consequent = consequent;
        Alternate = alternate;

        Children.Add(Test, Consequent);
        if (Alternate != null) Children.Add(Alternate);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return "|if> " + Stringify(new {
            Test = Test.ToString(),
            Consequent = Consequent.ToString(),
            Alternate = Alternate?.ToString(),
        }) + " <|";
    }
}
