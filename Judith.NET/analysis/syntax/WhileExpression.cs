﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public class WhileExpression : Expression {
    public Expression Test { get; init; }
    public Body Body { get; init; }

    public Token? WhileToken { get; init; }

    public WhileExpression (Expression test, Body body)
        : base(SyntaxKind.WhileExpression) {
        Test = test;
        Body = body;

        Children.Add(Test, Body);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
