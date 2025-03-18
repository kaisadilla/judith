using Judith.NET.analysis.lexical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class RawArrayType : TypeNode {
    public TypeNode MemberType { get; private init; }
    public Expression Length { get; private init; }

    public Token? LeftSquareBracketToken { get; set; }
    public Token? RightSquareBracketToken { get; set; }

    public RawArrayType (
        bool isConstant, bool isNullable, TypeNode memberType, Expression length
    )
        : base(SyntaxKind.RawArrayType, isConstant, isNullable)
    {
        MemberType = memberType;
        Length = length;

        Children.Add(MemberType, Length);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
