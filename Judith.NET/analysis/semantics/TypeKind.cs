using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Judith.NET.analysis.semantics;

// TODO: Remove?
[JsonConverter(typeof(StringEnumConverter))]
public enum TypeKind {
    Unresolved,
    Error,
    Pseudo,
    Primitive,
    Alias,
    String,
    Struct,
}