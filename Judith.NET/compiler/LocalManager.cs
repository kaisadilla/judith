using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler;

public class LocalManager {
    record Local (string Name, int depth) {
        public readonly string Name = Name;
        public readonly int Depth = depth;
        public bool Initialized = false;
    }

    /// <summary>
    /// The maximum amount of locals that can exist in the list at once.
    /// </summary>
    private int _maxLocals;

    /// <summary>
    /// The locals currently in scope.
    /// </summary>
    private List<Local> _locals = new();

    /// <summary>
    /// The amount of locals currently held by this manager.
    /// </summary>
    public int LocalCount => _locals.Count;

    public int ScopeDepth { get; set; } = 0;

    public LocalManager (int maxLocals) {
        _maxLocals = maxLocals;
    }

    /// <summary>
    /// Adds a local and returns its address.
    /// </summary>
    /// <param name="name">The name of the local.</param>
    /// <returns>The local slot the local will be in.</returns>
    public int AddLocal (string name) {
        if (_locals.Count >= _maxLocals) {
            throw new Exception("Too many locals."); // TODO: Compile error.
        }

        Local local = new(name, ScopeDepth);
        _locals.Add(local);
        return _locals.Count - 1;
    }

    /// <summary>
    /// Returns true if there's a local with the name given in the scope.
    /// </summary>
    /// <param name="name">The name of the local to test.</param>
    public bool IsLocalDeclared (string name) {
        foreach (var otherLocal in _locals) {
            if (otherLocal.Name == name) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Searches a local by name and returns whether it's been found. Its
    /// address is passed to the out argument.
    /// </summary>
    /// <param name="name">The name of the local to find.</param>
    /// <param name="addr">The address of the local, when found. 0 otherwise.</param>
    public bool TryGetLocalAddr (string name, out int addr) {
        for (int i = 0; i < _locals.Count; i++) {
            if (_locals[i].Name == name) {
                if (_locals[i].Initialized == false) {
                    throw new Exception("Local is not initialized!");
                }

                addr = i;
                return true;
            }
        }

        addr = 0;
        return false;
    }

    // TODO: Remove initialization flags from the compile step. This should be
    // checked for in the analysis step.
    public void MarkInitialized (int addr) {
        _locals[addr].Initialized = true;
    }
}
