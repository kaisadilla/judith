using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class ForeachExpression : Expression {
    public FieldDeclarationExpression Initializer { get; init; }
    public Expression Enumerable { get; init; }
    public Statement Body { get; init; }

    public Token? ForeachToken { get; init; }
    public Token? InToken { get; init; }

    public ForeachExpression (
        FieldDeclarationExpression initializer,
        Expression enumerable,
        Statement body
    )
        : base(SyntaxKind.ForeachExpression)
    {
        Initializer = initializer;
        Enumerable = enumerable;
        Body = body;

        Children.Add(Initializer, Enumerable, Body);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return "|foreach> " + Stringify(new {
            Initializer = Initializer.ToString(),
            Enumerable = Enumerable.ToString(),
            Body = Body.ToString(),
        }) + " <|";
    }
}

