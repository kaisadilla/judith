using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public class ForeachExpression : Expression {
    public List<LocalDeclarator> Declarators { get; init; }
    public Expression Enumerable { get; init; }
    public Body Body { get; init; }

    public Token? ForeachToken { get; init; }
    public Token? InToken { get; init; }

    public ForeachExpression (
        List<LocalDeclarator> declarators,
        Expression enumerable,
        Body body
    )
        : base(SyntaxKind.ForeachExpression) {
        Declarators = declarators;
        Enumerable = enumerable;
        Body = body;

        Children.AddRange(Declarators);
        Children.Add(Enumerable, Body);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}

