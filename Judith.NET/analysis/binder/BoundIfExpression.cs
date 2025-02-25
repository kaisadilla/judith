using Judith.NET.analysis.syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundIfExpression : BoundExpression {
    public new IfExpression Node => (IfExpression)base.Node;

    [JsonIgnore]
    public SymbolTable ConsequentScope { get; private init; }
    [JsonIgnore]
    public SymbolTable? AlternateScope { get; private init; }

    public BoundIfExpression (
        IfExpression node, SymbolTable consequentScope, SymbolTable alternateScope
    ) : base(node) {
        ConsequentScope = consequentScope;
        AlternateScope = alternateScope;
    }
}
