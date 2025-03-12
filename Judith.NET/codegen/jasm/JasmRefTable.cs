using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.codegen.jasm;

public class JasmRefTable {
    /// <summary>
    /// The references contained in this table.
    /// </summary>
    public List<JasmRef> Table { get; private init; } = new();

    /// <summary>
    /// Maps each unique name to its index in the table.
    /// </summary>
    private Dictionary<string, int> _dictionary = new();

    /// <summary>
    /// The amount of entries in the array.
    /// </summary>
    public int Size => Table.Count;

    /// <summary>
    /// Returns the FunctionRef at the index given.
    /// </summary>
    /// <param name="i">The index of the FunctionRef in the array.</param>
    public JasmRef this[int i] {
        get => Table[i];
    }

    public void Add (string name, JasmRef jasmRef) {
        if (_dictionary.ContainsKey(name)) {
            throw new($"Reference to '{name}' already exists.");
        }

        Table.Add(jasmRef);
        _dictionary[name] = Table.Count - 1;
    }

    public bool TryGetRefIndex (string name, out int index) {
        return _dictionary.TryGetValue(name, out index);
    }

    public bool TryGetRef (string name, [NotNullWhen(true)] out JasmRef? jasmRef) {
        if (TryGetRefIndex(name, out int index)) {
            jasmRef = Table[index];
            return true;
        }

        jasmRef = null;
        return false;
    }
}
