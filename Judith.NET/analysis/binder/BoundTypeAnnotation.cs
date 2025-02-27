using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundTypeAnnotation : BoundNode {
    public new TypeAnnotation Node => (TypeAnnotation)base.Node;

    public TypeSymbol Type { get; private init; }

    public BoundTypeAnnotation (TypeAnnotation node, TypeSymbol type) : base(node) {
        Type = type;
    }
}
