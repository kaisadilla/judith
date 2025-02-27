using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundYieldStatement : BoundNode {
    public new YieldStatement Node => (YieldStatement)base.Node;

    public TypeSymbol? Type { get; set; }

    public BoundYieldStatement (YieldStatement yieldStmt) : base(yieldStmt) { }
}
