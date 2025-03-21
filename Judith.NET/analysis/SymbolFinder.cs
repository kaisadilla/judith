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

        // Only the global scope can exist in more than one assembly at once so,
        // if this scope is not the global scope, then we can know the symbol
        // does not exist.
        if (originScope.IsGlobalTable == false) return false;

        if (_cmp.Program.NativeHeader.Types.ContainsKey(name)) return true;
        if (_cmp.Program.NativeHeader.Functions.ContainsKey(name)) return true;

        return false;
    }

    public List<Symbol> FindRecursively (
        string name, SymbolTable originScope, List<string> imports
    ) {
        Symbol? symbol = null;

        // Step 1: Try to find the symbol in the scope we are in. If we can't
        // find it there, search for it in that scope's parent, and so on.
        //
        // As modules are confined to a single assembly, we don't need to bother
        // checking dependencies, as we know the origin scope won't be found
        // there. The only exception to this is the global scope, which can
        // exist both in the script we are compiling and the native assembly.
        SymbolTable? scope = originScope;

        while (scope != null) {
            if (scope.TryGetSymbol(name, out symbol)) {
                return [symbol];
            }

            if (scope.IsGlobalTable) {
                if (_cmp.Program.NativeHeader.Types.TryGetValue(name, out var ts)) {
                    return [ts];
                }
                if (_cmp.Program.NativeHeader.Functions.TryGetValue(name, out var fs)) {
                    return [fs];
                }
            }

            scope = scope.Parent;
        }

        // Step 2: Try to find the symbol in the imports. This step is a bit
        // different. We only look for the symbol in the exact
        // module that is in imported, not any of its parents. e.g. if we
        // import "std::colletions" but not "std", then we only search the
        // "std/collections" scope.
        // We look in both this project and every dependency, as imports may
        // exist in any of these. We return as one as we find the module once as,
        // again, we know each module may only exist in one project.
        // It's possible that a symbol exists in more than one module,
        // and that's not an error - that's what modules are for.
        // For this reason, in this step we collect every symbol we find rather
        // than returning as soon as we find one - it's up to the caller to
        // decide what to do if multiple symbols are found for this query.
        List<Symbol> results = [];
        
        foreach (var import in imports) {
            // TODO: Search through imports.
        }

        return results;
    }
}
