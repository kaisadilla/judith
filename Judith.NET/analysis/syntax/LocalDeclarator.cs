using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public class LocalDeclarator : SyntaxNode {
    public Identifier Identifier { get; init; }
    public LocalKind LocalKind { get; init; }
    public TypeAnnotation? TypeAnnotation { get; private set; }
    /// <summary>
    /// Indicates whether the type annotation is inherited from another local
    /// declarator that follows this one, rather than being explicitly indicated
    /// for this specific declarator. For example, in the statement
    /// "var a, b: Num", b has its type "Num" explicitly indicated, while a
    /// inherits the type annotation "Num" from b. This is NOT inference.
    /// </summary>
    public bool IsTypeAnnotationInherited { get; set; }

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

    public void SetTypeAnnotation (TypeAnnotation? type) {
        TypeAnnotation = type;
    }
}
