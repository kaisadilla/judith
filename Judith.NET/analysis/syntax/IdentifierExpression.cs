using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Judith.NET.analysis.syntax;

public class IdentifierExpression : Expression {
    public Identifier Identifier { get; init; }

    public IdentifierExpression (Identifier identifier)
        : base(SyntaxKind.IdentifierExpression) {
        Identifier = identifier;

        Children.Add(Identifier);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
    public override string ToString () {
        return $"{Kind} ({Identifier.Name}) [Line: {Line}, Span: {Span.Start} - {Span.End}]";
    }
}
