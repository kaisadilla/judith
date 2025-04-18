﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRArgument : IRNode {
    public IRExpression Expression { get; private set; }

    public IRArgument (IRExpression expression) {
        Expression = expression;
    }
}
