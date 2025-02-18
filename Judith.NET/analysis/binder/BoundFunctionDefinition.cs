using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundFunctionDefinition : BoundNode {
    public new FunctionDefinition Node => (FunctionDefinition)base.Node;

    public Symbol Symbol { get; private init; }
    public SymbolTable Scope { get; private init; }
    public List<TypeInfo>? ParameterTypes { get; set; } = null;
    public TypeInfo? ReturnType { get; set; } = null;

    public bool IsComplete => IsTypeInfoResolved; // TODO: ParameterTypes
    public bool IsTypeInfoResolved => TypeInfo.IsResolved(ReturnType);

    public BoundFunctionDefinition (
        FunctionDefinition node, Symbol symbol, SymbolTable scope
    ) : base(node) {
        Symbol = symbol;
        Scope = scope;
    }
}
