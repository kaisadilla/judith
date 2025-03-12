using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRCallExpression : IRExpression {
    public IRExpression Callee { get; private set; }
    public List<IRArgument> Arguments { get; private set; }

    public IRCallExpression (
        IRExpression callee, List<IRArgument> arguments, string type
    )
        : base(type)
    {
        Callee = callee;
        Arguments = arguments;
    }
}
