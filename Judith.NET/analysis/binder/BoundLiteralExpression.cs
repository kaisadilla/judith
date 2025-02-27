using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundLiteralExpression : BoundExpression {
    public new LiteralExpression Node => (LiteralExpression)base.Node;

    public ConstantValue Value { get; private set; }

    public BoundLiteralExpression (
        LiteralExpression node, TypeSymbol typeInfo, ConstantValue value
    )
        : base(node)
    {
        Type = typeInfo;
        Value = value;
    }
}
