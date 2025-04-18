﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public class WhenStatement : Statement {
    public Statement Statement { get; private init; }
    public Expression Test { get; private init; }

    public Token? WhenToken { get; init; }

    public WhenStatement (Statement statement, Expression test)
        : base(SyntaxKind.WhenStatement) {
        Statement = statement;
        Test = test;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
