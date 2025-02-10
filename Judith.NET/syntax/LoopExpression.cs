using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class LoopExpression : Expression {
    public Statement Body { get; init; }

    public Token? LoopToken { get; init; }

    public LoopExpression (Statement body) : base(SyntaxKind.LoopExpression) {
        Body = body;

        Children.Add(Body);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return "|loop> " + Stringify(new { Body = Body.ToString() }) + " <|";
    }
}
