using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class FunctionDefinition : Item {
    public bool IsHidden { get; private init; }
    public Identifier Identifier { get; private init; }
    public ParameterList Parameters { get; private init; }
    public TypeAnnotation? ReturnType { get; private init; }
    public Statement Body { get; private init; }

    public Token? HidToken { get; init; }
    public Token? FuncToken { get; init; }
    public Token? ColonToken { get; init; }

    public FunctionDefinition (
        bool isHidden,
        Identifier name,
        ParameterList parameters,
        TypeAnnotation? returnType,
        Statement body
    )
        : base(SyntaxKind.FunctionDefinition)
    {
        IsHidden = isHidden;
        Identifier = name;
        Parameters = parameters;
        ReturnType = returnType;
        Body = body;

        Children.Add(Identifier, Parameters, Body);

        if (ReturnType != null) Children.Add(ReturnType);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return "|ITEM function> " + Stringify(new {
            Name = Identifier.ToString(),
            Parameters = Parameters.ToString(),
            ReturnType = ReturnType?.ToString(),
            Body = Body.ToString(),
        }) + " <|";
    }
}
