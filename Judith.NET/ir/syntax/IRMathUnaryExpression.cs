using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRMathUnaryExpression : IRExpression {
    public IRExpression Expression { get; private set; }
    public IRUnaryOperation Operation { get; private set; }

    public IRMathUnaryExpression (
        IRExpression expr, IRUnaryOperation operation, string type
    )
        : base(type)
    {
        Expression = expr;
        Operation = operation;
    }
}
