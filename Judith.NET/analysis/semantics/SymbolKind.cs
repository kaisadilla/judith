using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Judith.NET.analysis.semantics;

[JsonConverter(typeof(StringEnumConverter))]
public enum SymbolKind {
    Module,
    UnresolvedPseudoType,
    ErrorPseudoType,
    PseudoType,
    PrimitiveType,
    StringType,
    CharType,
    FunctionType,
    AliasType,
    UnionType,
    SetType,
    StructType,
    InterfaceType,
    ClassType,
    Namespace,
    Function, // Symbols that define functions, not any kind of variables of type function.
    FunctionOverload,
    MemberField,
    MemberFunction,
    Parameter,
    Local,
}