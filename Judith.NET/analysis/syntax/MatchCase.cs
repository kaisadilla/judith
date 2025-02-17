using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class MatchCase : SyntaxNode {
    public List<Expression> Tests { get; init; }
    public Statement Consequent { get; init; }
    public bool IsElseCase { get; init; }

    public Token? ElseToken { get; init; }

    public MatchCase (List<Expression> tests, Statement consequent, bool isElseCase)
        : base(SyntaxKind.MatchCase) {
        Tests = tests;
        Consequent = consequent;
        IsElseCase = isElseCase;

        Children.AddRange(Tests);
        Children.Add(Consequent);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
