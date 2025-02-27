using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundParameter : BoundNode {
    public new Parameter Node => (Parameter)base.Node;

    public Symbol Symbol { get; private init; }
    public TypeSymbol? Type { get; set; } = null;

    public BoundParameter (Parameter param, Symbol symbol)
        : base(param) {
        Symbol = symbol;
    }
}
