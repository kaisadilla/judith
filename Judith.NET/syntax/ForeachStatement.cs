using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class ForeachStatement : Statement {
    public FieldDeclarationExpression Initializer { get; init; }
    public Expression Enumerable { get; init; }
    public Statement Body { get; init; }

    public Token? ForeachToken { get; init; }
    public Token? InToken { get; init; }

    public ForeachStatement (
        FieldDeclarationExpression initializer,
        Expression enumerable,
        Statement body
    )
        : base(SyntaxKind.ForeachStatement)
    {
        Initializer = initializer;
        Enumerable = enumerable;
        Body = body;
    }

    public override string ToString () {
        return "|foreach> " + Stringify(new {
            Initializer = Initializer.ToString(),
            Enumerable = Enumerable.ToString(),
            Body = Body.ToString(),
        }) + " <|";
    }
}

