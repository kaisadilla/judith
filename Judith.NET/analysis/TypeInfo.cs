using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

public class TypeInfo {
    public string Name { get; set; }
    public string FullyQualifiedName { get; set; }

    public static TypeInfo UnresolvedType { get; private set; } = new(
        "!Unresolved", "!Unresolved"
    );

    public static TypeInfo VoidType { get; private set; } = new(
        "Void", "Void"
    );

    public static TypeInfo ErrorType { get; private set; } = new(
        "<error-type>", "<error-type>"
    );

    public static bool IsResolved ([NotNullWhen(true)] TypeInfo? typeInfo) {
        return typeInfo != null && typeInfo != UnresolvedType;
    }

    public TypeInfo (string name, string fullyQualifiedName) {
        Name = name;
        FullyQualifiedName = fullyQualifiedName;
    }
}
