using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public class MatchExpression : Expression {
    public Expression Discriminant { get; init; }
    public List<MatchCase> Cases { get; init; }

    public Token? MatchToken { get; init; }
    public Token? DoToken { get; init; }
    public Token? EndToken { get; init; }

    public MatchExpression (Expression discriminant, List<MatchCase> cases)
        : base(SyntaxKind.MatchExpression) {
        Discriminant = discriminant;
        Cases = cases;

        Children.Add(Discriminant);
        Children.AddRange(Cases);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
