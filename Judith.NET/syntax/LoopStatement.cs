using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class LoopStatement : Statement {
    public Statement Body { get; init; }

    public Token? LoopToken { get; init; }

    public LoopStatement (Statement body) : base(SyntaxKind.LoopStatement) {
        Body = body;
    }

    public override string ToString () {
        return "|loop> " + Stringify(new { Body = Body.ToString() }) + " <|";
    }
}
