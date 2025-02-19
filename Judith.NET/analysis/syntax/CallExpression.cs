using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class CallExpression : Expression {
    public Expression Callee { get; init; }
    public ArgumentList Arguments { get; init; }

    public CallExpression (Expression callee, ArgumentList arguments)
        : base(SyntaxKind.CallExpression)
    {
        Callee = callee;
        Arguments = arguments;

        Children.Add(Callee, Arguments);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
