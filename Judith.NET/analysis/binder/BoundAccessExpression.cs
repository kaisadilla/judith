using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class BoundAccessExpression : BoundExpression, IBoundIdentifyingExpression {
    public new AccessExpression Node => (AccessExpression)base.Node;

    public TypeSymbol? AssociatedType { get; } = null;

    /// <summary>
    /// The symbol that is the member being accessed.
    /// </summary>
    public MemberSymbol MemberSymbol { get; }

    public Symbol Symbol => MemberSymbol;

    public BoundAccessExpression (AccessExpression node, MemberSymbol memberSymbol)
        : base(node)
    {
        MemberSymbol = memberSymbol;
    }
}
