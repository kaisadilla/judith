using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRIfExpression : IRExpression {
    public IRExpression Test { get; private set; }
    public List<IRStatement> Consequent { get; private set; }
    public List<IRStatement>? Alternate { get; private set; }

    public IRIfExpression (
        IRExpression test,
        List<IRStatement> consequent,
        List<IRStatement>? alternate,
        IRTypeName type
    )
        : base(type)
    {
        Test = test;
        Consequent = consequent;
        Alternate = alternate;
    }
}
