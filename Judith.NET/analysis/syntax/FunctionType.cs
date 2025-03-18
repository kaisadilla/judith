using Judith.NET.analysis.lexical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class FunctionType : TypeNode {
    public List<TypeNode> ParameterTypes { get; private init; }
    public TypeNode ReturnType { get; private init; }

    public Token? LeftParenthesisToken { get; init; }
    public Token? RightParenthesisToken { get; init; }

    public FunctionType (
        bool isConstant,
        bool isNullable,
        List<TypeNode> parameterTypes,
        TypeNode returnType
    )
        : base(SyntaxKind.FunctionType, isConstant, isNullable)
    {
        ParameterTypes = parameterTypes;
        ReturnType = returnType;

        Children.AddRange(ParameterTypes);
        Children.Add(ReturnType);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
