using Judith.NET.codegen;
using Judith.NET.ir.syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir;

public class IRProgramResolver {
    private IRProgram _program;

    /// <summary>
    /// A cache for types that have already been resolved.
    /// </summary>
    private Dictionary<string, IRType> _typeCache = [];
    private Dictionary<string, IRFunction> _functionCache = [];

    public IRProgramResolver (IRProgram program) {
        _program = program;
    }

    /// <summary>
    /// Returns the IR type identified by the name given.
    /// If the type cannot be found in the IR program or any of its dependencies,
    /// an exception is thrown.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <returns></returns>
    /// <exception cref="InvalidIRProgramException"></exception>
    public IRType GetIRType (string name) {
        // If the type is cached, return it.
        if (_typeCache.TryGetValue(name, out IRType? type)) {
            return type;
        }

        // Search it in the native header, and return it if it's found.
        if (_program.NativeHeader.Types.TryGetValue(name, out type)) {
            return type;
        }

        // Search it in the program we are compiling, and return it if it's found.
        foreach (var block in _program.Blocks) {
            foreach (var internalType in block.Types) {
                if (internalType.Name == name) {
                    _typeCache[name] = internalType;
                    return internalType;
                }
            }
        }

        // Search it inside the dependencies, and return it if it's found.
        foreach (var dep in _program.Dependencies) {
            if (dep.Types.TryGetValue(name, out type)) {
                _typeCache[name] = type;
                return type;
            }
        }

        // If it isn't found anywhere, then the IR Program is invalid.
        throw new InvalidIRProgramException($"Cannot resolve type name '{name}'");
    }

    public bool TryGetIRFunction (
        string name, [NotNullWhen(true)] out IRFunction? function
    ) {
        // If the type is cached, return it.
        if (_functionCache.TryGetValue(name, out function)) {
            return true;
        }

        // Search it in the native header, and return it if it's found.
        if (_program.NativeHeader.Functions.TryGetValue(name, out function)) {
            return true;
        }

        // Search it in the program we are compiling, and return it if it's found.
        foreach (var block in _program.Blocks) {
            foreach (var internalFunc in block.Functions) {
                if (internalFunc.Name == name) {
                    _functionCache[name] = internalFunc;
                    function = internalFunc;
                    return true;
                }
            }
        }

        // Search it inside the dependencies, and return it if it's found.
        foreach (var dep in _program.Dependencies) {
            if (dep.Functions.TryGetValue(name, out function)) {
                _functionCache[name] = function;
                return true;
            }
        }

        // The name doesn't exist as a function in this program.
        function = null;
        return false;
    }

    /// <summary>
    /// Returns the IR function identified by the name given.
    /// If the function cannot be found in the IR program or any of its
    /// dependencies, an exception is thrown.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <returns></returns>
    /// <exception cref="InvalidIRProgramException"></exception>
    public IRFunction GetIRFunction (string name) {
        if (TryGetIRFunction(name, out IRFunction? function)) {
            return function;
        }

        // If the function cannot be found, then the IR Program is invalid.
        throw new InvalidIRProgramException($"Cannot resolve type name '{name}'");
    }
}
