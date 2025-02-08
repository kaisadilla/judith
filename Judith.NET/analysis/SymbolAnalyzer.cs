using Judith.NET.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

public class SymbolAnalyzer : SyntaxVisitor {
    public void DoTheVisitStuff (List<SyntaxNode> nodes) {
        foreach (var node in nodes) {
            node.Accept(this);
        }
    }

    public override void Visit (FunctionItem node) {
        Console.WriteLine("FUNC! " + node.Name.RawToken!.Lexeme);
    }
}
