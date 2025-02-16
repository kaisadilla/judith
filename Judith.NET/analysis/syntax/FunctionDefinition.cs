using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class FunctionDefinition : Item {
    /// <summary>
    /// True when this function is built implicitly from top-level statements.
    /// </summary>
    public bool IsImplicit { get; private init; }
    public bool IsHidden { get; private init; }
    public Identifier Identifier { get; private init; }
    public ParameterList Parameters { get; private init; }
    public TypeAnnotation? ReturnTypeAnnotation { get; private init; }
    public Statement Body { get; private init; }

    public Token? HidToken { get; init; }
    public Token? FuncToken { get; init; }
    public Token? ColonToken { get; init; }

    public FunctionDefinition (
        bool isImplicit,
        bool isHidden,
        Identifier name,
        ParameterList parameters,
        TypeAnnotation? returnType,
        Statement body
    )
        : base(SyntaxKind.FunctionDefinition) {
        IsImplicit = isImplicit;
        IsHidden = isHidden;
        Identifier = name;
        Parameters = parameters;
        ReturnTypeAnnotation = returnType;
        Body = body;

        Children.Add(Identifier, Parameters, Body);

        if (ReturnTypeAnnotation != null) Children.Add(ReturnTypeAnnotation);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }

    public override string ToString () {
        return "|ITEM function> " + Stringify(new {
            Name = Identifier.ToString(),
            Parameters = Parameters.ToString(),
            ReturnType = ReturnTypeAnnotation?.ToString(),
            Body = Body.ToString(),
        }) + " <|";
    }
}
