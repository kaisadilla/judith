using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.semantics;

public class TypeSymbol : Symbol {
    public TypeSymbol (
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
    public static new DefinerFunc<TypeSymbol> Define (SymbolKind kind, string name) {
        return table => new TypeSymbol(table, kind, name, table.Qualify(name));
    }

    public static bool IsResolved (TypeSymbol? symbol) {
        return symbol != null && symbol.Kind != SymbolKind.UnresolvedType;
    }
}