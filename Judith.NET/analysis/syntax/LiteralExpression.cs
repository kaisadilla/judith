﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class LiteralExpression : Expression {
    public Literal Literal { get; private set; }

    public LiteralExpression (Literal literal) : base(SyntaxKind.LiteralExpression) {
        Literal = literal;

        Children.Add(Literal);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }

    public override string ToString () {
        return $"{Kind} ({Literal.Source}) [Line: {Line}, Span: {Span.Start} - {Span.End}]";
    }
}
