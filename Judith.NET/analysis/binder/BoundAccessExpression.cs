using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundAccessExpression : BoundExpression, IBoundIdentifyingExpression {
    public BoundAccessExpression (SyntaxNode node) : base(node) {
    }

    public Symbol Symbol => throw new NotImplementedException();

    public TypeSymbol? AssociatedType => throw new NotImplementedException();
}
