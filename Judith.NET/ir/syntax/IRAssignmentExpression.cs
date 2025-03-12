using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRAssignmentExpression : IRExpression {
    public IRExpression Left { get; private set; }
    public IRExpression Right { get; private set; }

    public IRAssignmentExpression (
        IRExpression left, IRExpression right, string type
    )
        : base(type)
    {
        Left = left;
        Right = right;
    }
}
