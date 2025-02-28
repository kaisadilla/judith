using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundStructTypeDefinition : BoundNode {
    public new StructTypeDefinition Node => (StructTypeDefinition)base.Node;

    public TypeSymbol Symbol { get; private init; }
    [JsonIgnore]
    public SymbolTable Scope { get; private init; }

    public BoundStructTypeDefinition (
        StructTypeDefinition node, TypeSymbol symbol, SymbolTable scope
    )
        : base(node)
    {
        Symbol = symbol;
        Scope = scope;
    }
}
