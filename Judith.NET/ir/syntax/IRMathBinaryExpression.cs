using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRMathBinaryExpression : IRExpression {
    public IRExpression Left { get; private set; }
    public IRExpression Right { get; private set; }
    public IRMathOperation Operation { get; private set; }

    public IRMathBinaryExpression (
        IRExpression left, IRExpression right, IRMathOperation operation, string type
    )
        : base(type)
    {
        Left = left;
        Right = right;
        Operation = operation;
    }
}
