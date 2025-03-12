using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRReturnStatement : IRStatement {
    public IRExpression? Expression { get; private set; }

    public IRReturnStatement (IRExpression? expression) {
        Expression = expression;
    }
}
