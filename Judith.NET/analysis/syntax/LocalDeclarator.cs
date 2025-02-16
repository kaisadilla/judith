using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class LocalDeclarator : SyntaxNode {
    public Identifier Identifier { get; init; }
    public LocalKind LocalKind { get; init; }
    public TypeAnnotation? TypeAnnotation { get; private set; }

    public Token? FieldKindToken { get; init; }

    public LocalDeclarator (
        Identifier identifier, LocalKind localKind, TypeAnnotation? typeAnnotation
    )
        : base(SyntaxKind.LocalDeclarator) {
        Identifier = identifier;
        LocalKind = localKind;
        TypeAnnotation = typeAnnotation;

        Children.Add(Identifier);
        if (TypeAnnotation != null) Children.Add(TypeAnnotation);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }

    public override string ToString () {
        string str = $"[{LocalKind}] {Identifier}";

        if (TypeAnnotation != null) {
            str += $": {TypeAnnotation}";
        }

        return str;
    }

    public void SetType (TypeAnnotation? type) {
        TypeAnnotation = type;
    }
}
