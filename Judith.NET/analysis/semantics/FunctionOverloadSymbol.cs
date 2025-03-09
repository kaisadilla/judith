using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.semantics;

public class FunctionOverloadSymbol : Symbol {
    /// <summary>
    /// The function symbol that contains this overload.
    /// </summary>
    [JsonIgnore]
    public FunctionSymbol Function { get; private init; }

    public List<TypeSymbol> ParamTypes { get; private init; } = new();
    public TypeSymbol? ReturnType { get; set; }
    public bool IsDuplicate { get; set; } = false;

    public FunctionOverloadSymbol (
        SymbolTable table,
        FunctionSymbol functionSymbol,
        List<TypeSymbol> paramTypes,
        string name
    )
        : base(table, SymbolKind.FunctionOverload, name, table.Qualify(name))
    {
        Function = functionSymbol;
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
        if (IsResolved() == false || TypeSymbol.IsResolved(ReturnType) == false) {
            return "{{unresolved}}";
        }

        var sb = new StringBuilder(Function.Name + "(");

        foreach (var type in ParamTypes) {
            sb.Append(type.SignatureName);
        }
        sb.Append(')');

        sb.Append(ReturnType.SignatureName);

        return sb.ToString();
    }
}
