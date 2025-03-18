using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class RawArrayType : TypeNode {
    public TypeNode MemberType { get; private init; }

    public RawArrayType (TypeNode memberType) : base(SyntaxKind.RawArrayType) {
        MemberType = memberType;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
