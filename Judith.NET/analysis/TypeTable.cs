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
    private Dictionary<string, TypeInfo> _types = new();

    public bool ContainsType (string fullyQualifiedName) {
        return _types.ContainsKey(fullyQualifiedName);
    }

    public bool TryGetType (
        string fullyQualifiedName, [NotNullWhen(true)] out TypeInfo? type
    ) {
        return _types.TryGetValue(fullyQualifiedName, out type);
    }

    public void AddType (TypeInfo type) {
        if (_types.ContainsKey(type.FullyQualifiedName)) {
            throw new Exception($"Type '{type.FullyQualifiedName}' already exists.");
        }

        _types[type.FullyQualifiedName] = type;
    }
}
