using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundLeftUnaryExpression : BoundExpression {
    public new LeftUnaryExpression Node => (LeftUnaryExpression)base.Node;

    public override bool IsComplete => TypeInfo.IsResolved(Type);

    public BoundLeftUnaryExpression (LeftUnaryExpression leftUnaryExpr) : base(leftUnaryExpr) { }
}
