﻿using Judith.NET.analysis.semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public interface IBoundIdentifyingExpression {
    public Symbol Symbol { get; }

    public TypeSymbol? Type { get; }

    /// <summary>
    /// The type generated by the symbol identified by this expression. This
    /// is null when the expression identifies symbols that don't generate a
    /// type (for example, a local). However, when this expression identifies
    /// a typedef symbol, this contains the identified symbol.
    /// </summary>
    public TypeSymbol? AssociatedType { get; }
}
