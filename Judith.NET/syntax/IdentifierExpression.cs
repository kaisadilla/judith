using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class IdentifierExpression : Expression {
    public Identifier Identifier { get; init; }

    public IdentifierExpression (Identifier identifier)
        : base(SyntaxKind.IdentifierExpression)
    {
        Identifier = identifier;

        Children.Add(Identifier);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return $"{Identifier}";
    }
}
