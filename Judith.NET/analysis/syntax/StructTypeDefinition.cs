using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class StructTypeDefinition : TypeDefinition {
    public bool IsHidden { get; private init; }
    public Identifier Identifier { get; private init; }
    public List<MemberField> MemberFields { get; private init; }

    public Token? HidToken { get; init; }
    public Token? StructToken { get; set; }
    public Token? EndToken { get; set; }

    public StructTypeDefinition (
        bool isHidden, Identifier identifier, List<MemberField> memberFields
    )
        : base(SyntaxKind.StructTypeDefinition)
    {
        IsHidden = isHidden;
        Identifier = identifier;
        MemberFields = memberFields;

        Children.Add(identifier);
        Children.AddRange(memberFields);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
