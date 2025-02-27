using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Judith.NET.analysis.semantics;

public class FunctionSymbol : Symbol {

    public List<FunctionOverload> Overloads { get; private set; } = [];

    public FunctionSymbol (
        SymbolTable table,
        string name,
        string fullyQualifiedName
    )
        : base(table, SymbolKind.Function, name, fullyQualifiedName) { }

    /// <summary>
    /// Returns a function that will create symbols with the kind and name given.
    /// </summary>
    /// <param name="name">The name of the symbol.</param>
    /// <returns></returns>
    public static DefinerFunc<FunctionSymbol> Define (string name) {
        return table => new FunctionSymbol(table, name, table.Qualify(name));
    }

    /// <summary>
    /// Finds the overload that matches the parameter types given (in the same
    /// order). This function will not check overloads that aren't fully
    /// resolved, so failure to find an overload shouldn't be taken as an error.
    /// If two identical overloads exist (which is an error), the first one
    /// found will be returned.
    /// </summary>
    /// <param name="paramTypes">A list of parameter types, in order.</param>
    /// <returns></returns>
    public bool TryGetOverload (
        List<TypeSymbol> paramTypes, [NotNullWhen(true)] out FunctionOverload? overload
    ) {
        foreach (var ol in Overloads) {
            if (ol.MatchesParamTypes(paramTypes)) {
                overload = ol;
                return true;
            }
        }

        overload = null;
        return false;
    }

    public bool AreAllResolved () {
        foreach (var overload in Overloads) {
            if (overload.IsResolved() == false) return false;
        }

        return true;
    }

    /// <summary>
    /// Analyzes the types of all overloads and returns true if two identical
    /// overloads are found (which is invalid).
    /// </summary>
    public bool AnalyzeOverloads () {
        bool collision = false;

        for (int a = 0; a < Overloads.Count; a++) {
            for (int b = a; b < Overloads.Count; b++) {
                if (AreListsEqual(Overloads[a].ParamTypes, Overloads[b].ParamTypes)) {
                    Overloads[a].IsDuplicate = true;
                    Overloads[b].IsDuplicate = true;
                    collision = true;
                }
            }
        }

        return collision;
    }

    private static bool AreListsEqual (List<TypeSymbol> a, List<TypeSymbol> b) {
        if (a.Count != b.Count) return false;

        for (int i = 0; i < a.Count; i++) {
            if (ReferenceEquals(a[i], b[i]) == false) return false;
        }

        return true;
    }
}

public class FunctionOverload {
    /// <summary>
    /// The function symbol that contains this overload.
    /// </summary>
    [JsonIgnore]
    public FunctionSymbol Function { get; private init; }

    public List<TypeSymbol> ParamTypes { get; private init; } = new();
    public TypeSymbol? ReturnType { get; set; }
    public bool IsDuplicate { get; set; } = false;

    public FunctionOverload (FunctionSymbol symbol, List<TypeSymbol> paramTypes) {
        Function = symbol;
        ParamTypes = paramTypes;
    }

    /// <summary>
    /// Returns true if the overload given matches the one in this symbol. If
    /// either overload contains unresolved types, this function returns false.
    /// </summary>
    /// <param name="paramTypes">The type of each parameter, in order.</param>
    public bool MatchesParamTypes (List<TypeSymbol> paramTypes) {
        if (paramTypes.Count != ParamTypes.Count) return false;

        for (int i = 0; i < paramTypes.Count; i++) {
            if (TypeSymbol.IsResolved(paramTypes[i]) == false) return false;
            if (paramTypes[i] != ParamTypes[i]) return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if this function's overload types are all resolved.
    /// </summary>
    public bool IsResolved () {
        foreach (var type in ParamTypes) {
            if (TypeSymbol.IsResolved(type) == false) return false;
        }

        return true;
    }

    public string GetSignatureString () {
        //return Symbol.FullyQualifiedName + "("; 
        return "--placeholder"; // TODO: Replace with something like "(std/Vec3;II)V".
    }
}
