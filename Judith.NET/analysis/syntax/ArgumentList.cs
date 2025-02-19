using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class ArgumentList : SyntaxNode {
    public List<Argument> Arguments { get; set; }


    public Token? LeftParenthesisToken { get; init; }
    public Token? RightParenthesisToken { get; init; }

    public ArgumentList (List<Argument> arguments) : base(SyntaxKind.ArgumentList) {
        Arguments = arguments;

        Children.AddRange(Arguments);
    }


    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
