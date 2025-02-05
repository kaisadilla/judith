using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class IfStatement : Statement {
    public Expression Test { get; init; }
    public Statement Consequent { get; init; }
    public Statement? Alternate { get; init; }

    public Token? IfToken { get; init; }
    public Token? ElseToken { get; init; }

    public IfStatement (Expression test, Statement consequent, Statement? alternate)
        : base(SyntaxKind.IfStatement)
    {
        Test = test;
        Consequent = consequent;
        Alternate = alternate;
    }

    public override string ToString () {
        //return $"|if>test:> {Test} <:, consequent:> {Consequent} <:, " +
        //    $"alternate:> {Alternate} <:<|";

        return "|if> " + Stringify(new {
            Test = Test.ToString(),
            Consequent = Consequent.ToString(),
            Alternate = Alternate?.ToString(),
        }) + " <|";
    }
}
