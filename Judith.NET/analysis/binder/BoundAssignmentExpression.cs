using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundAssignmentExpression : BoundExpression {
    public new AssignmentExpression Node => (AssignmentExpression)base.Node;

    public BoundAssignmentExpression (AssignmentExpression assignmentExpr) : base(assignmentExpr) { }
}
