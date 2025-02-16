using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundBinaryExpression : BoundExpression {
    public new BinaryExpression Node => (BinaryExpression)base.Node;

    public override bool IsComplete => TypeInfo.IsResolved(Type);

    public BoundBinaryExpression (BinaryExpression binaryExpr) : base(binaryExpr) { }
}
