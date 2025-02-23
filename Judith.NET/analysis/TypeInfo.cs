using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

[JsonConverter(typeof(StringEnumConverter))]
public enum TypeKind {
    Unresolved,
    Pseudo,
    Primitive,
    Alias,
    String,
    Struct,
}

public class TypeInfo {
    public TypeKind Kind { get; private init; }
    public string Name { get; private init; }
    public string FullyQualifiedName { get; private init; }

    public static TypeInfo UnresolvedType { get; private set; } = new(
        TypeKind.Unresolved, "!Unresolved", "!Unresolved"
    );

    public static TypeInfo VoidType { get; private set; } = new(
        TypeKind.Pseudo, "Void", "Void"
    );

    public static TypeInfo ErrorType { get; private set; } = new(
        TypeKind.Pseudo, "<error-type>", "<error-type>"
    );

    public static bool IsResolved ([NotNullWhen(true)] TypeInfo? typeInfo) {
        return typeInfo != null && typeInfo != UnresolvedType;
    }

    public TypeInfo (TypeKind kind, string name, string fullyQualifiedName) {
        Kind = kind;
        Name = name;
        FullyQualifiedName = fullyQualifiedName;
    }
}

// TODO: Specialized type infos: AliasedTypeInfo, UnionTypeInfo, SetTypeInfo, etc.