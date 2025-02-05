using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class MatchStatement : Statement {
    public Expression Discriminant { get; init; }
    public List<MatchCase> Cases { get; init; }

    public Token? MatchToken { get; init; }
    public Token? DoToken { get; init; }
    public Token? EndToken { get; init; }

    public MatchStatement (Expression discriminant, List<MatchCase> cases)
        : base(SyntaxKind.MatchStatement)
    {
        Discriminant = discriminant;
        Cases = cases;
    }

    public override string ToString () {
        return "|match> " + Stringify(new {
            Discriminant = Discriminant.ToString(),
            Cases = Cases.Select(c => c.ToString()),
        }) + " <|";
    }
}
