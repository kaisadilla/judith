using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public class LoopExpression : Expression {
    public Body Body { get; init; }

    public Token? LoopToken { get; init; }

    public LoopExpression (Body body) : base(SyntaxKind.LoopExpression) {
        Body = body;

        Children.Add(Body);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
