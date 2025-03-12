using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRExpressionStatement : IRStatement {
    public IRExpression Expression { get; private set; }

    public IRExpressionStatement (IRExpression expression) {
        Expression = expression;
    }
}
