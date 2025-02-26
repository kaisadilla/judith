using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

public class MemberDescription {
    public string Name { get; private init; }
    public Symbol Symbol { get; private init; }

    public MemberDescription (Symbol symbol) {
        Name = symbol.Name;
        Symbol = symbol;
    }
}
