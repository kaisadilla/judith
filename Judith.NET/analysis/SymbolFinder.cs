using Judith.NET.analysis.semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Judith.NET.analysis;

public class SymbolFinder {
    JudithCompilation _cmp;

    public SymbolFinder (JudithCompilation cmp) {
        _cmp = cmp;
    }

    /// <summary>
    /// Returns true if the symbol given already exists in the current scope.
    /// This does not check parent scopes.
    /// </summary>
    /// <param name="name">The name to search.</param>
    /// <param name="originScope">The scope from which it's searched.</param>
    /// <returns></returns>
    public bool IsSymbolDefinedInScope (string name, SymbolTable originScope) {
        // If the scope contains the symbol, then it is defined.
        if (originScope.ContainsSymbol(name)) return true;

        // If the scope cannot be split (basically, every kind of scope other
        // than modules), then the scope doesn't contain the symbol.
        if (originScope.CanBeSplit == false || originScope.Symbol == null) {
            return false;
        }

        // Dependencies will be searched by fully qualified name.
        var fqn = originScope.Qualify(name);

        if (_cmp.Program.NativeHeader.Types.ContainsKey(fqn)) return true;
        if (_cmp.Program.NativeHeader.Functions.ContainsKey(fqn)) return true;

        foreach (var dep in _cmp.Program.Dependencies) {
            if (dep.Types.ContainsKey(fqn)) return true;
            if (dep.Functions.ContainsKey(fqn)) return true;
        }

        return false;
    }

    public List<Symbol> FindRecursively (
        string name, SymbolTable originScope, List<string> imports
    ) {
        Symbol? symbol = null;

        // Step 1: Try to find the symbol in the scope we are in. If we can't
        // find it there, search for it in that scope's parent, and so on.
        //
        // We may or may not be in a module scope. If we are not, then we are
        // only concerned about the scope directly defined in the source (for
        // example, the scope of a function) can only exist in the place the
        // function is defined. However, if we are in a module scope, then that
        // module may also exist in the dependencies (this won't usually be the
        // case, but it's allowed). In this case, we have to try to find the
        // symbol in the scope for that module defined not only in this
        // compilation, but also in each of its dependencies.
        // To do this, we'll start in the current scope. In each iteration, we'll
        // try to find the same scope inside the dependencies, and store it at
        // their index in the "dependencyScopes" variable (remember that this
        // only happens when the current scope is a module). If we find one name,
        // then we return a list containing that name - there can't be more than
        // one definition of the symbol, so we don't need to look further.
        // Afterwards, if we haven't returned, we'll assign the scope's parent
        // to "scope", and repeat the process. Sooner or later we'll reach the
        // top table (the global namespace), and after that iteration is complete,
        // "scope" will become "null" and the loop will end.
        SymbolTable? scope = originScope;

        while (scope != null) {
            if (scope.TryGetSymbol(name, out symbol)) {
                return [symbol];
            }

            if (scope.CanBeSplit && scope.Symbol != null) {
                var fqn = scope.Qualify(name);

                if (_cmp.Program.NativeHeader.Types.TryGetValue(fqn, out var ts)) {
                    return [ts];
                }
                if (_cmp.Program.NativeHeader.Functions.TryGetValue(fqn, out var fs)) {
                    return [fs];
                }

                foreach (var dep in _cmp.Program.Dependencies) {
                    if (dep.Types.TryGetValue(fqn, out ts)) {
                        return [ts];
                    }
                    if (dep.Functions.TryGetValue(fqn, out fs)) {
                        return [fs];
                    }
                }
            }

            scope = scope.Parent;
        }

        // Step 2: Try to find the symbol in the imports. This step is a bit
        // different. First of all, we only look for the symbol in the exact
        // module that is in imported, not any of its parents. e.g. if we
        // import "std::colletions" but not "std", then we only search the
        // "std/collections" scope.
        // Second, it's possible that a symbol exists in more than one module,
        // and that's not an error - that's what modules are for.
        // For this reason, in this step we collect every symbol we find rather
        // than returning as soon as we find one.

        List<Symbol> results = [];
        
        foreach (var import in imports) {
            // TODO: Search through imports.
        }

        return results;
    }
}
