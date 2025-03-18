using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class FunctionType : TypeNode {
    public List<TypeNode> ParameterTypes { get; private init; }
    public TypeNode ReturnType { get; private init; }

    public FunctionType (List<TypeNode> parameterTypes, TypeNode returnType)
        : base(SyntaxKind.FunctionType)
    {
        ParameterTypes = parameterTypes;
        ReturnType = returnType;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
