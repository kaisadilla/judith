using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundObjectInitializer : BoundNode {
    public new ObjectInitializer Node => (ObjectInitializer)base.Node;

    public SymbolTable Scope { get; private init; }

    public BoundObjectInitializer (ObjectInitializer node, SymbolTable scope)
        : base(node)
    {
        Scope = scope;
    }
}
