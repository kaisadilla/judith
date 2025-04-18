﻿using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public abstract class BoundExpression : BoundNode {
    public TypeSymbol? Type { get; set; }

    public BoundExpression (SyntaxNode node) : base(node) { }
}
