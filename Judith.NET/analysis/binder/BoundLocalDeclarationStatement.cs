using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundLocalDeclarationStatement : BoundNode {
    public new LocalDeclarationStatement Node => (LocalDeclarationStatement)base.Node;

    public BoundLocalDeclarationStatement (LocalDeclarationStatement node)
        : base(node)
    {}
}
