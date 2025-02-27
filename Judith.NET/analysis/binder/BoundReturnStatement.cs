using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundReturnStatement : BoundNode {
    public new ReturnStatement Node => (ReturnStatement)base.Node;

    public TypeSymbol? Type { get; set; }

    public BoundReturnStatement (ReturnStatement returnStmt) : base(returnStmt) { }
}
