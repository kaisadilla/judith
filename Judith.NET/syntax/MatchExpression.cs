using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class MatchExpression : Expression {
    public Expression Discriminant { get; init; }
    public List<MatchCase> Cases { get; init; }

    public Token? MatchToken { get; init; }
    public Token? DoToken { get; init; }
    public Token? EndToken { get; init; }

    public MatchExpression (Expression discriminant, List<MatchCase> cases)
        : base(SyntaxKind.MatchExpression)
    {
        Discriminant = discriminant;
        Cases = cases;

        Children.Add(Discriminant);
        Children.AddRange(Cases);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return "|match> " + Stringify(new {
            Discriminant = Discriminant.ToString(),
            Cases = Cases.Select(c => c.ToString()),
        }) + " <|";
    }
}
