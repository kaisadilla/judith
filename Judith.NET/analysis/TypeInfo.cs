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
    Error,
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

    /// <summary>
    /// This pseudotype represents a type that has been resolved to a function,
    /// but whose specific overload has not been resolved.
    /// </summary>
    public static TypeInfo UnresolvedFunctionType { get; private set; } = new(
        TypeKind.Unresolved, "!Function", "!Function"
    );

    /// <summary>
    /// This error type represents the type of an invalid expression that cannot
    /// be evaluated (e.g. referring to a non-existant local).
    /// </summary>
    public static TypeInfo ErrorType { get; private set; } = new(
        TypeKind.Error, "<error-type>", "<error-type>"
    );

    /// <summary>
    /// This pseudotype represents the "type" of something that doesn't have
    /// a type at all. For example, a type itself doesn't have a type, so doing
    /// "5 + Num" is invalid, because "Num" doesn't have a type. Unlike Void,
    /// which means something could have a type but doesn't, this means that
    /// something could not possibly have a type and, while syntactically that
    /// something can be used somewhere where a type is needed, semantically
    /// such use doesn't make sense.
    /// </summary>
    public static TypeInfo NoType { get; private set; } = new(
        TypeKind.Pseudo, "<no-type>", "<no-type>"
    );

    /// <summary>
    /// This pseudotype represents elements that generate their own type. For
    /// example, an object initializer of the kind "{a = b}" is anonymous.
    /// </summary>
    public static TypeInfo AnonymousObject { get; private set; } = new(
        TypeKind.Pseudo, "<anonymous-object>", "<anonymous-object>"
    );

    /// <summary>
    /// This pseudotype represents a (valid) type that is used when a type
    /// must be provided, but at the same time the context doesn't contain
    /// a type. For example, the return type of a function that doesn't return
    /// anything is "Void".
    /// </summary>
    public static TypeInfo VoidType { get; private set; } = new(
        TypeKind.Pseudo, "Void", "Void"
    );

    public static bool IsResolved ([NotNullWhen(true)] TypeInfo? typeInfo) {
        return typeInfo != null && typeInfo.Kind != TypeKind.Unresolved;
    }

    public TypeInfo (TypeKind kind, string name, string fullyQualifiedName) {
        Kind = kind;
        Name = name;
        FullyQualifiedName = fullyQualifiedName;
    }
}

// TODO: Specialized type infos: AliasedTypeInfo, UnionTypeInfo, SetTypeInfo, etc.