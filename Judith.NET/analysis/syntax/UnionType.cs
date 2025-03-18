using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class UnionType : TypeNode {
    public List<TypeNode> MemberTypes { get; private init; }

    public UnionType (List<TypeNode> memberTypes) : base(SyntaxKind.UnionType) {
        MemberTypes = memberTypes;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
