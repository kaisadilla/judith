using Judith.NET.diagnostics.serialization;
using Newtonsoft.Json;

namespace Judith.NET.analysis.semantics;

[JsonConverter(typeof(SymbolJsonConverter))]
public class Symbol {
    public SymbolKind Kind { get; set; }

    public string Name { get; set; }

    public string FullyQualifiedName { get; set; }

    public string Assembly { get; set; }

    /// <summary>
    /// The type this symbol resolves to when used. Locals will resolve to the
    /// type they were assigned, functions will resolve to function types, and
    /// other types (such as typedefs) resolve to "no type".
    /// This CANNOT be used to resolve the type of a typedef, as the symbol
    /// itself is the type.
    /// </summary>
    public TypeSymbol? Type { get; set; }

    public Symbol (
        SymbolKind kind, string name, string fullyQualifiedName, string assembly
    ) {
        Kind = kind;
        Name = name;
        FullyQualifiedName = fullyQualifiedName;
        Assembly = assembly;
    }
}