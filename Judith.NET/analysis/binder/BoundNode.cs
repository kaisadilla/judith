using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public abstract class BoundNode {
    public SyntaxNode Node { get; private set; }

    protected BoundNode (SyntaxNode node) {
        Node = node;
    }
}
