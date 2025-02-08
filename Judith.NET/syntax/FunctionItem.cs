using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class FunctionItem : Item {
    public Identifier Name { get; init; }
    public ParameterList Parameters { get; init; }
    public IdentifierExpression? ReturnType { get; init; }
    public Statement Body { get; init; }

    public Token? FuncToken { get; init; }
    public Token? ColonToken { get; init; }

    public FunctionItem (
        Identifier name,
        ParameterList parameters,
        IdentifierExpression? returnType,
        Statement body
    )
        : base(SyntaxKind.FunctionItem)
    {
        Name = name;
        Parameters = parameters;
        ReturnType = returnType;
        Body = body;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return "|ITEM function> " + Stringify(new {
            Name = Name.ToString(),
            Parameters = Parameters.ToString(),
            ReturnType = ReturnType?.ToString(),
            Body = Body.ToString(),
        }) + " <|";
    }
}
