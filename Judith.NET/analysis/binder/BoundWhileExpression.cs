using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundWhileExpression : BoundExpression {
    public new WhileExpression Node => (WhileExpression)base.Node;

    public SymbolTable BodyScope { get; private init; }

    public override bool IsComplete => TypeInfo.IsResolved(Type);

    public BoundWhileExpression (WhileExpression node, SymbolTable bodyScope) : base(node) {
        BodyScope = bodyScope;
    }
}
