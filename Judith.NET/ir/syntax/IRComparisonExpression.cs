using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRComparisonExpression : IRExpression {
    public IRExpression Left { get; private set; }
    public IRExpression Right { get; private set; }
    public IRComparisonOperation Operation { get; private set; }

    public IRComparisonExpression (
        IRExpression left, IRExpression right, IRComparisonOperation operation, string type
    )
        : base(type)
    {
        Left = left;
        Right = right;
        Operation = operation;
    }
}
