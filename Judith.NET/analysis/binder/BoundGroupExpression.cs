using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundGroupExpression : BoundExpression {
    public new GroupExpression Node => (GroupExpression)base.Node;

    public override bool IsComplete => TypeInfo.IsResolved(Type);

    public BoundGroupExpression (GroupExpression groupExpr) : base(groupExpr) {}
}
