using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class TypeAnnotation : SyntaxNode {
    public Identifier Identifier { get; private init; }

    public Token? ColonToken { get; init; }

    public TypeAnnotation (Identifier identifier) : base(SyntaxKind.TypeAnnotation) {
        Identifier = identifier;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return ": " + Identifier.ToString();
    }
}
