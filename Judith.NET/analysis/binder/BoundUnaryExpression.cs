using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundUnaryExpression : BoundExpression {
    public new UnaryExpression Node => (UnaryExpression)base.Node;

    public BoundUnaryExpression (UnaryExpression unaryExpr) : base(unaryExpr) { }
}
