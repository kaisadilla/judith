﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class LiteralType : TypeNode {
    public Literal Literal { get; private init; }

    public LiteralType (bool isConstant, bool isNullable, Literal literal)
        : base(SyntaxKind.LiteralType, isConstant, isNullable)
    {
        Literal = literal;

        Children.Add(Literal);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
