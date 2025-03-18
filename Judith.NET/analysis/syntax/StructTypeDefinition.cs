using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public class StructTypeDefinition : TypeDefinition {
    public SimpleIdentifier Identifier { get; private init; }
    public List<MemberField> MemberFields { get; private init; }

    public Token? StructToken { get; set; }
    public Token? EndToken { get; set; }

    public StructTypeDefinition (
        bool isHidden, SimpleIdentifier identifier, List<MemberField> memberFields
    )
        : base(SyntaxKind.StructTypeDefinition, isHidden)
    {
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
