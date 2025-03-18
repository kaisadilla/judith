using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class IdentifierType : TypeNode {
    public Identifier Name { get; private init; }

    public IdentifierType (bool isConstant, bool isNullable, Identifier name)
        : base(SyntaxKind.IdentifierType, isConstant, isNullable)
    {
        Name = name;

        Children.Add(Name);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
