using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class ObjectType : TypeNode {
    public ObjectType (bool isConstant, bool isNullable)
        : base(SyntaxKind.ObjectType, isConstant, isNullable)
    {
        throw new NotImplementedException();
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
