using Judith.NET.analysis.lexical;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

[JsonConverter(typeof(StringEnumConverter))]
public enum MemberAccessKind {
    /// <summary>
    /// The member can be read but not mutated from outside the enclosing structure.
    /// </summary>
    ReadOnly,
    /// <summary>
    /// The member can be read and mutated from outside the enclosing structure.
    /// </summary>
    Public,
    /// <summary>
    /// The member cannot be read nor mutated from outside the enclosing structure.
    /// </summary>
    Hidden,
}

public class MemberField : SyntaxNode {
    public MemberAccessKind Access { get; private set; }
    public bool IsStatic { get; private set; }
    public bool IsMutable { get; private set; }
    public bool IsConst { get; private set; }
    public Identifier Identifier { get; private set; }
    public TypeAnnotation TypeAnnotation { get; private set; }
    public EqualsValueClause? Initializer { get; private set; }

    public Token? AccessToken { get; set; }
    public Token? StaticToken { get; set; }
    public Token? MutableToken { get; set; }
    public Token? ConstToken { get; set; }

    public MemberField (
        MemberAccessKind access,
        bool isStatic,
        bool isMutable,
        bool isConst,
        Identifier identifier,
        TypeAnnotation annotation,
        EqualsValueClause? initializer
    )
        : base(SyntaxKind.MemberField)
    {
        Access = access;
        IsStatic = isStatic;
        IsMutable = isMutable;
        IsConst = isConst;
        Identifier = identifier;
        TypeAnnotation = annotation;
        Initializer = initializer;

        Children.Add(identifier, annotation);
        if (initializer != null) Children.Add(initializer);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
