using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Judith.NET.analysis.semantics;

[JsonConverter(typeof(StringEnumConverter))]
public enum SymbolKind {
    Module,
    UnresolvedType,
    ErrorType,
    PseudoType,
    PrimitiveType,
    StringType,
    CharType,
    AliasType,
    UnionType,
    SetType,
    StructType,
    InterfaceType,
    ClassType,
    Namespace,
    Function, // Symbols that define functions, not any kind of variables of type function.
    MemberField,
    MemberFunction,
    Parameter,
    Local,
}