using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public class EqualsValueClause : SyntaxNode {
    public Expression Value { get; init; }
    public Token? EqualsToken { get; init; }

    public EqualsValueClause (Expression value) : base(SyntaxKind.EqualsValueClause) {
        Value = value;

        Children.Add(Value);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
