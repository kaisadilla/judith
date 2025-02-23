using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;
public class BoundMemberField : BoundNode {
    public new MemberField Node => (MemberField)base.Node;

    public Symbol Symbol { get; private init; }
    public TypeInfo? Type { get; set; } = null;

    public BoundMemberField (MemberField node, Symbol symbol) : base(node) {
        Symbol = symbol;
    }
}
