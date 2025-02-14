using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class CompilerUnit : SyntaxNode {
    // public List<ImportDirective> Imports { get; private init; }
    // public ModuleDirective? Module { get; private init; }
    public List<Item> TopLevelItems { get; private init; }
    public FunctionDefinition? ImplicitFunction { get; private set; }

    public CompilerUnit (List<Item> topLevelItems, FunctionDefinition? implicitFunction)
        : base(SyntaxKind.CompilerUnit)
    {
        TopLevelItems = topLevelItems;
        ImplicitFunction = implicitFunction;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return "|compiler unit> " + Stringify(new {
            TopLevelItems = TopLevelItems.Select(i => i.ToString()),
            ImplicitFunction = ImplicitFunction?.ToString(),
        }) + " <|";
    }
}
