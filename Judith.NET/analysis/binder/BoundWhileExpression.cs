using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundWhileExpression : BoundExpression {
    public new WhileExpression Node => (WhileExpression)base.Node;

    [JsonIgnore]
    public SymbolTable BodyScope { get; private init; }

    public BoundWhileExpression (WhileExpression node, SymbolTable bodyScope) : base(node) {
        BodyScope = bodyScope;
    }
}
