using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

/// <summary>
/// Traverses all the nodes in a compilation unit, creating all the types
/// defined in it.
/// </summary>
public class TypeTable {
    public Dictionary<string, TypeInfo> Types { get; private set; } = new();

    public bool ContainsType (string fullyQualifiedName) {
        return Types.ContainsKey(fullyQualifiedName);
    }

    public bool TryGetType (
        string fullyQualifiedName, [NotNullWhen(true)] out TypeInfo? type
    ) {
        return Types.TryGetValue(fullyQualifiedName, out type);
    }

    public void AddType (TypeInfo type) {
        if (Types.ContainsKey(type.FullyQualifiedName)) {
            throw new Exception($"Type '{type.FullyQualifiedName}' already exists.");
        }

        Types[type.FullyQualifiedName] = type;
    }
}
