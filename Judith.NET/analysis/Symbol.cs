using Judith.NET.analysis.serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

[JsonConverter(typeof(StringEnumConverter))]
public enum SymbolKind {
    Module,
    NativeType,
    AliasType,
    UnionType,
    SetType,
    StructType,
    InterfaceType,
    Class,
    Namespace,
    Function,
    MemberField,
    MemberFunction,
    Local,
}

[JsonConverter(typeof(SymbolJsonConverter))]
public class Symbol {
    /// <summary>
    /// The symbol table this symbol belongs to.
    /// </summary>
    public SymbolTable Table { get; set; }
    public SymbolKind Kind { get; set; }
    public string Name { get; set; }
    public string FullyQualifiedName { get; set; }
    public TypeInfo? Type { get; set; }

    public Symbol (SymbolTable table, SymbolKind kind, string name, string fullyQualifiedName) {
        Table = table;
        Kind = kind;
        Name = name;
        FullyQualifiedName = fullyQualifiedName;
    }
}
