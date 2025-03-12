using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Judith.NET.analysis.semantics;

public class FunctionSymbol : Symbol {
    public List<TypeSymbol> ParamTypes { get; private init; }
    public TypeSymbol? ReturnType { get; set; }

    public FunctionSymbol (
        List<TypeSymbol> paramTypes,
        string name,
        string fullyQualifiedName,
        string assembly
    )
        : base(SymbolKind.Function, name, fullyQualifiedName, assembly)
    {
        ParamTypes = paramTypes;
    }

    /// <summary>
    /// Returns true if this function's overload types are all resolved.
    /// </summary>
    public bool AreParamsResolved () {
        foreach (var type in ParamTypes) {
            if (TypeSymbol.IsResolved(type) == false) return false;
        }

        return true;
    }
}
