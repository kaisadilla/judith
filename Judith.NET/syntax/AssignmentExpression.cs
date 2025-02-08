﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class AssignmentExpression : Expression {
    public Expression Left { get; init; }
    public Expression Right { get; init; }

    public Token? EqualsToken { get; init; }

    public AssignmentExpression (
        Expression left, Expression right
    )
        : base(SyntaxKind.AssignmentExpression)
    {
        Left = left;
        Right = right;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return $"({Left} = {Right})";
    }
}
