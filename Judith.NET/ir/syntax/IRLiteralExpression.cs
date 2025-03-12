using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRLiteralExpression : IRExpression {
    public ConstantValue Value { get; private init; }

    public IRLiteralExpression (ConstantValue value, string type) : base(type) {
        Value = value;
    }
}
