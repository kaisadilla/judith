using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundStructTypeDefinition : BoundNode {
    public new StructTypeDefinition Node => (StructTypeDefinition)base.Node;

    public Symbol Symbol { get; private init; }
    public SymbolTable Scope { get; private init; }
    public TypeInfo? Type { get; set; } = null;

    public BoundStructTypeDefinition (
        StructTypeDefinition node, Symbol symbol, SymbolTable scope
    )
        : base(node)
    {
        Symbol = symbol;
        Scope = scope;
    }
}
