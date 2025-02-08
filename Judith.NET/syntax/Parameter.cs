using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class Parameter : SyntaxNode {
    public Identifier Identifier { get; init; }
    public FieldKind FieldKind { get; init; }
    public IdentifierExpression? Type { get; init; }
    public EqualsValueClause? DefaultValue { get; init; }

    public Token? FieldKindToken { get; init; }

    public Parameter (
        Identifier identifier,
        FieldKind fieldKind,
        IdentifierExpression? type,
        EqualsValueClause? defaultValue
    )
        : base(SyntaxKind.Parameter)
    {
        Identifier = identifier;
        FieldKind = fieldKind;
        Type = type;
        DefaultValue = defaultValue;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        string str = $"[{FieldKind}] {Identifier}";

        if (Type != null) {
            str += $": {Type}";
        }

        if (DefaultValue != null) {
            str += $" = {DefaultValue}";
        }

        return str;
    }
}
