using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public abstract class IRExpression : IRNode {
    public IRTypeName Type { get; private init; }

    protected IRExpression (IRTypeName type) {
        Type = type;
    }
}
