using Judith.NET.diagnostics.serialization;
using Newtonsoft.Json;

namespace Judith.NET.analysis.semantics;

[JsonConverter(typeof(SymbolJsonConverter))]
public class Symbol {
    /// <summary>
    /// A function that contains the name and kind of the symbol to create.
    /// </summary>
    /// <param name="table">The table to use to create the symbol.</param>
    public delegate T DefinerFunc<T> (SymbolTable table);

    /// <summary>
    /// The symbol table this symbol belongs to.
    /// </summary>
    public SymbolTable Table { get; set; }

    public SymbolKind Kind { get; set; }

    public string Name { get; set; }

    public string FullyQualifiedName { get; set; }

    /// <summary>
    /// The type this symbol resolves to when used. Locals will resolve to the
    /// type they were assigned, functions will resolve to function types, and
    /// other types (such as typedefs) resolve to "no type".
    /// This CANNOT be used to resolve the type of a typedef, as the symbol
    /// itself is the type.
    /// </summary>
    public TypeSymbol? Type { get; set; }

    public Symbol (
        SymbolTable table, SymbolKind kind, string name, string fullyQualifiedName
    ) {
        Table = table;
        Kind = kind;
        Name = name;
        FullyQualifiedName = fullyQualifiedName;
    }

    /// <summary>
    /// Returns a function that will create symbols with the kind and name given.
    /// </summary>
    /// <param name="kind">The kind of symbol.</param>
    /// <param name="name">The name of the symbol.</param>
    /// <returns></returns>
    public static DefinerFunc<Symbol> Define (SymbolKind kind, string name) {
        return table => new Symbol(table, kind, name, table.Qualify(name));
    }
}