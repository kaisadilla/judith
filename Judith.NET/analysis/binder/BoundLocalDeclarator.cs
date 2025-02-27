using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundLocalDeclarator : BoundNode {
    public new LocalDeclarator Node => (LocalDeclarator)base.Node;

    public Symbol Symbol { get; private init; }
    public TypeSymbol? Type { get; set; } = null;

    public BoundLocalDeclarator (LocalDeclarator localDecl, Symbol symbol)
        : base(localDecl)
    {
        Symbol = symbol;
    }
}
