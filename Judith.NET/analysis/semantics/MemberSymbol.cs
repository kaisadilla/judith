using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.semantics;

public class MemberSymbol : Symbol {
    public MemberSymbol (
        SymbolKind kind, string name, string fullyQualifiedName, string assembly
    )
        : base(kind, name, fullyQualifiedName, assembly)
    {}
}
