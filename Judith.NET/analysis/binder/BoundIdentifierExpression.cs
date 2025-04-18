﻿using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public enum IdentifierKind {
    Type,
    Local,
}

public class BoundIdentifierExpression : BoundExpression, IBoundIdentifyingExpression {
    public new IdentifierExpression Node => (IdentifierExpression)base.Node;

    public Symbol Symbol { get; private init; }

    public TypeSymbol? AssociatedType { get; set; }

    public BoundIdentifierExpression (IdentifierExpression idExpr, Symbol symbol)
        : base(idExpr)
    {
        Symbol = symbol;
    }
}
