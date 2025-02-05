﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class MatchCase : SyntaxNode {
    public List<Expression> Tests { get; init; }
    public Statement Consequent { get; init; }
    public bool IsElseCase { get; init; }

    public Token? ElseToken { get; init; }

    public MatchCase (List<Expression> tests, Statement consequent, bool isElseCase)
        : base(SyntaxKind.MatchCase)
    {
        Tests = tests;
        Consequent = consequent;
        IsElseCase = isElseCase;
    }

    public override string ToString () {
        return "|match case> " + Stringify(new {
            Tests = Tests.Select(t => t.ToString()),
            Consequent = Consequent.ToString(),
            IsElseCase,
        }) + " <|";
    }
}
