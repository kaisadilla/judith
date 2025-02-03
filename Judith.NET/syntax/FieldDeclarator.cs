using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;
public class FieldDeclarator : SyntaxNode {
    public Identifier Identifier { get; init; }
    public FieldKind FieldKind { get; init; }
    public IdentifierExpression? Type { get; private set; }
    
    public Token? KindToken { get; init; }

    public FieldDeclarator (Identifier identifier, FieldKind fieldKind, IdentifierExpression? type)
        : base(SyntaxKind.FieldDeclarator)
    {
        Identifier = identifier;
        FieldKind = fieldKind;
        Type = type;
    }

    public override string ToString () {
        string str = $"[{FieldKind}] {Identifier}";

        if (Type != null) {
            str += $": {Type}";
        }

        return str;
    }

    public void SetType (IdentifierExpression? type) {
        Type = type;
    }
}
