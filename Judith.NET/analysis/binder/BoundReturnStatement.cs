using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundReturnStatement : BoundNode {
    public new ReturnStatement Node => (ReturnStatement)base.Node;

    public TypeInfo? Type { get; set; }

    public BoundReturnStatement (ReturnStatement returnStmt) : base(returnStmt) { }
}
