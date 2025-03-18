using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class TupleArrayType : SyntaxNode {
    public List<TypeNode> MemberTypes { get; private init; }

    public TupleArrayType (List<TypeNode> memberTypes)
        : base(SyntaxKind.TupleArrayType)
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
