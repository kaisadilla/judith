using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IR_P_PrintStatement : IRStatement {
    public IRExpression Expression { get; private set; }

    public IR_P_PrintStatement (IRExpression expression) {
        Expression = expression;
    }
}
