﻿using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundAccessExpression : BoundExpression {
    public BoundAccessExpression (SyntaxNode node) : base(node) {
    }
}
