using Judith.NET.analysis.lexical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class GroupType : TypeNode {
    public TypeNode Type { get; init; }
    public Token? LeftParenthesisToken { get; init; }
    public Token? RightParenthesisToken { get; init; }

    public GroupType (bool isConstant, bool isNullable, TypeNode type)
        : base(SyntaxKind.GroupType, isConstant, isNullable)
    {
        Type = type;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
