using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class UnionType : TypeNode {
    public List<TypeNode> MemberTypes { get; private init; }

    public UnionType (bool isConstant, bool isNullable, List<TypeNode> memberTypes)
        : base(SyntaxKind.UnionType, isConstant, isNullable)
    {
        MemberTypes = memberTypes;

        Children.AddRange(MemberTypes);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
