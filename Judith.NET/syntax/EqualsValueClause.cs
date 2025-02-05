using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class EqualsValueClause : SyntaxNode {
    public Expression Value { get; init; }
    public Token? EqualsToken { get; init; }

    public EqualsValueClause (Expression value) : base(SyntaxKind.EqualsValueClause) {
        Value = value;
    }

    public override string ToString () {
        return $"(= {Value})";
    }
}
