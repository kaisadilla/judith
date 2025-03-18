using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public class FunctionDefinition : Item {
    /// <summary>
    /// True when this function is built implicitly from top-level statements.
    /// </summary>
    public bool IsImplicit { get; private init; }
    public bool IsHidden { get; private init; }
    public SimpleIdentifier Identifier { get; private init; }
    public ParameterList Parameters { get; private init; }
    public TypeAnnotation? ReturnTypeAnnotation { get; private init; }
    public BlockStatement Body { get; private init; }

    public Token? HidToken { get; init; }
    public Token? FuncToken { get; init; }
    public Token? ColonToken { get; init; }

    public FunctionDefinition (
        bool isImplicit,
        bool isHidden,
        SimpleIdentifier name,
        ParameterList parameters,
        TypeAnnotation? returnType,
        BlockStatement body
    )
        : base(SyntaxKind.FunctionDefinition)
    {
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
}
