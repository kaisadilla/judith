using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRWhileExpression : IRExpression {
    public IRExpression Test { get; private set; }
    public List<IRStatement> Body { get; private set; }

    public IRWhileExpression (IRExpression test, List<IRStatement> body, string type)
        : base(type)
    {
        Test = test;
        Body = body;
    }
}
