using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundFunctionDefinition : BoundNode {
    public new FunctionDefinition Node => (FunctionDefinition)base.Node;

    public FunctionSymbol Symbol { get; private init; }
    public FunctionOverload Overload { get; private init; }
    [JsonIgnore]
    public SymbolTable Scope { get; private init; }

    public BoundFunctionDefinition (
        FunctionDefinition node,
        FunctionSymbol symbol,
        FunctionOverload overload,
        SymbolTable scope
    )
        : base(node)
    {
        Symbol = symbol;
        Overload = overload;
        Scope = scope;
    }
}
