using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundObjectInitializationExpression : BoundExpression {
    public new ObjectInitializationExpression Node
        => (ObjectInitializationExpression)base.Node;

    public BoundObjectInitializationExpression (ObjectInitializationExpression node)
        : base(node)
    {}
}
