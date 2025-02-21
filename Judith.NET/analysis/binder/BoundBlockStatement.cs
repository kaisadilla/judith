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
    public new BlockStatement Node => (BlockStatement)base.Node;

    public BlockEvaluationKind EvaluationKind { get; set; }
    public TypeInfo? Type { get; set; }

    public BoundBlockStatement (BlockStatement blockStmt) : base(blockStmt) { }
}
