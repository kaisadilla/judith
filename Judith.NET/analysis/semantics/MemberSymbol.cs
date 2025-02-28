using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.semantics;

public class MemberSymbol : Symbol {
    public MemberSymbol (
        SymbolTable table, SymbolKind kind, string name, string fullyQualifiedName
    )
        : base(table, kind, name, fullyQualifiedName)
    {}

    /// <summary>
    /// Returns a function that will create symbols with the kind and name given.
    /// </summary>
    /// <param name="kind">The kind of symbol.</param>
    /// <param name="name">The name of the symbol.</param>
    /// <returns></returns>
    public static new DefinerFunc<MemberSymbol> Define (SymbolKind kind, string name) {
        return table => new MemberSymbol(table, kind, name, table.Qualify(name));
    }
}
