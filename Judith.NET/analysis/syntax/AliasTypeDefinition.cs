using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class AliasTypeDefinition : TypeDefinition {
    public Identifier Name { get; private init; }
    public Identifier AliasedType { get; private init; }
    public bool IsExplicit { get; private init; }

    public AliasTypeDefinition (
        bool isHidden, Identifier name, Identifier aliasedType, bool isExplicit
    )
        : base(SyntaxKind.AliasTypeDefinition, isHidden)
    {
        Name = name;
        AliasedType = aliasedType;
        IsExplicit = isExplicit;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
