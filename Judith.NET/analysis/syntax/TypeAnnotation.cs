using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public class TypeAnnotation : SyntaxNode {
    public TypeNode Type { get; private init; }

    public Token? Delimiter { get; init; }

    public TypeAnnotation (TypeNode type) : base(SyntaxKind.TypeAnnotation) {
        Type = type;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
