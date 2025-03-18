using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Judith.NET.analysis.syntax;

public class IdentifierExpression : Expression {
    public Identifier Name { get; init; }

    public IdentifierExpression (Identifier qualifiedName)
        : base(SyntaxKind.IdentifierExpression)
    {
        Name = qualifiedName;

        Children.Add(Name);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
    public override string ToString () {
        return $"{Kind} ({Name.FullyQualifiedName()}) [Line: {Line}, " +
            $"Span: {Span.Start} - {Span.End}]";
    }
}
