using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jub;

public class FunctionRefTable {
    /// <summary>
    /// Contains all the function references available in this program.
    /// </summary>
    public List<FunctionRef> Array { get; private init; } = new();

    /// <summary>
    /// Maps fully qualified function names to their FunctionRef.
    /// </summary>
    private Dictionary<string, int> _dictionary = new();

    /// <summary>
    /// The amount of entries in the array.
    /// </summary>
    public int Size => Array.Count;

    /// <summary>
    /// Returns the FunctionRef at the index given.
    /// </summary>
    /// <param name="i">The index of the FunctionRef in the array.</param>
    public FunctionRef this[int i] {
        get => Array[i];
    }

    public void Add (string fullyQualifiedName, FunctionRef functionRef) {
        if (_dictionary.ContainsKey(fullyQualifiedName)) {
            throw new($"Function definition for '{fullyQualifiedName}' already exists.");
        }
        Array.Add(functionRef);
        _dictionary[fullyQualifiedName] = Array.Count - 1;
    }

    public bool TryGetFunctionRef (string fullyQualifiedName, out int index) {
        return _dictionary.TryGetValue(fullyQualifiedName, out index);
    }
}
