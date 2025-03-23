using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public class EqualsValueClause : SyntaxNode {
    public List<Expression> Values { get; init; }
    public Token? EqualsToken { get; init; }

    public EqualsValueClause (List<Expression> value)
        : base(SyntaxKind.EqualsValueClause)
    {
        Values = value;

        Children.AddRange(Values);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
