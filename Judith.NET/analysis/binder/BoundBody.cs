using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public enum BlockEvaluationKind {
    Return,
    Yield,
}

public class BoundBody : BoundNode {
    public new Body Node => (Body)base.Node;

    public BlockEvaluationKind EvaluationKind { get; set; } // TODO: Probably irrelevant
    public TypeSymbol? Type { get; set; }

    public BoundBody (Body body) : base(body) { }
}
