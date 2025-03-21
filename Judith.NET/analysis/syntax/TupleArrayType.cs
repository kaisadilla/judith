﻿using Judith.NET.analysis.lexical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class TupleArrayType : TypeNode {
    public List<TypeNode> MemberTypes { get; private init; }

    public Token? LeftSquareBracketToken { get; set; }
    public Token? RightSquareBracketToken { get; set; }

    public TupleArrayType (bool isConstant, bool isNullable, List<TypeNode> memberTypes)
        : base(SyntaxKind.TupleArrayType, isConstant, isNullable)
    {
        MemberTypes = memberTypes;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
