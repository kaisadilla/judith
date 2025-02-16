using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class ForeachExpression : Expression
{
    public List<LocalDeclarator> Declarators { get; init; }
    public Expression Enumerable { get; init; }
    public Statement Body { get; init; }

    public Token? ForeachToken { get; init; }
    public Token? InToken { get; init; }

    public ForeachExpression(
        List<LocalDeclarator> declarators,
        Expression enumerable,
        Statement body
    )
        : base(SyntaxKind.ForeachExpression)
    {
        Declarators = declarators;
        Enumerable = enumerable;
        Body = body;

        Children.AddRange(Declarators);
        Children.Add(Enumerable, Body);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "|foreach> " + Stringify(new
        {
            Declarators = Declarators.Select(d => d.ToString()),
            Enumerable = Enumerable.ToString(),
            Body = Body.ToString(),
        }) + " <|";
    }
}

