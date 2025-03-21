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

public class BoundBlockStatement : BoundNode {
    public new BlockBody Node => (BlockBody)base.Node;

    public BlockEvaluationKind EvaluationKind { get; set; }
    public TypeSymbol? Type { get; set; }

    public BoundBlockStatement (BlockBody blockStmt) : base(blockStmt) { }
}
