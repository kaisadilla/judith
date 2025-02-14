using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class LocalDeclarator : SyntaxNode {
    public Identifier Identifier { get; init; }
    public LocalKind LocalKind { get; init; }
    public TypeAnnotation? Type { get; private set; }

    public Token? FieldKindToken { get; init; }

    public LocalDeclarator (
        Identifier identifier, LocalKind localKind, TypeAnnotation? type
    )
        : base(SyntaxKind.LocalDeclarator)
    {
        Identifier = identifier;
        LocalKind = localKind;
        Type = type;

        Children.Add(Identifier);
        if (Type != null) Children.Add(Type);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        string str = $"[{LocalKind}] {Identifier}";

        if (Type != null) {
            str += $": {Type}";
        }

        return str;
    }

    public void SetType (TypeAnnotation? type) {
        Type = type;
    }
}
