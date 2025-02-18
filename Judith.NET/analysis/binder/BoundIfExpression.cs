using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundIfExpression : BoundExpression {
    public new IfExpression Node => (IfExpression)base.Node;

    public SymbolTable ConsequentScope { get; private init; }
    public SymbolTable? AlternateScope { get; private init; }

    public override bool IsComplete => TypeInfo.IsResolved(Type);

    public BoundIfExpression (
        IfExpression node, SymbolTable consequentScope, SymbolTable alternateScope
    ) : base(node) {
        ConsequentScope = consequentScope;
        AlternateScope = alternateScope;
    }
}
