using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class ParameterList : SyntaxNode {
    public List<Parameter> Parameters { get; init; }

    public Token? LeftParenthesisToken { get; init; }
    public Token? RightParenthesisToken { get; init; }

    public ParameterList (List<Parameter> parameters) : base (SyntaxKind.ParameterList) {
        Parameters = parameters;

        Children.AddRange(Parameters);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return "( " + Stringify(Parameters.Select(p => p.ToString())) + " )";
    }
}
