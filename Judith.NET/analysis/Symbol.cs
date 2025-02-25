using Judith.NET.diagnostics.serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

[JsonConverter(typeof(StringEnumConverter))]
public enum SymbolKind {
    Module,
    NativeType,
    AliasType,
    UnionType,
    SetType,
    StructType,
    InterfaceType,
    ClassType,
    Namespace,
    Function, // Symbols that define functions, not any kind of variables of type function.
    MemberField,
    MemberFunction,
    Parameter,
    Local,
}

[JsonConverter(typeof(SymbolJsonConverter))]
public class Symbol {
    /// <summary>
    /// The symbol table this symbol belongs to.
    /// </summary>
    public SymbolTable Table { get; set; }
    public SymbolKind Kind { get; set; }
    public string Name { get; set; }
    public string FullyQualifiedName { get; set; }
    /// <summary>
    /// The type this symbol resolves to when used. Locals will resolve to the
    /// type they were assigned, functions will resolve to function types, and
    /// other types (such as typedefs) resolve to <see cref="TypeInfo.NoType"/>.
    /// This CANNOT be used to resolve the type of a typedef.
    /// </summary>
    public TypeInfo? Type { get; set; } // TODO: For functions, this is the type of the function signature (e.g. (Int, Int) => Void), not the return type, as that's determined by overloads.

    public Symbol (
        SymbolTable table, SymbolKind kind, string name, string fullyQualifiedName
    ) {
        Table = table;
        Kind = kind;
        Name = name;
        FullyQualifiedName = fullyQualifiedName;
    }
}

public class FunctionSymbol : Symbol {
    public List<FunctionOverload> Overloads { get; private set; } = [];

    public FunctionSymbol (
        SymbolTable table,
        string name,
        string fullyQualifiedName
    )
        : base(table, SymbolKind.Function, name, fullyQualifiedName)
    {}

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
        List<TypeInfo> paramTypes, [NotNullWhen(true)] out FunctionOverload? overload
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

    private static bool AreListsEqual (List<TypeInfo> a, List<TypeInfo> b) {
        if (a.Count != b.Count) return false;

        for (int i = 0; i < a.Count; i++) {
            if (ReferenceEquals(a[i], b[i]) == false) return false;
        }

        return true;
    }
}

public class TypedefSymbol : Symbol {
    /// <summary>
    /// The type associated to this struct.
    /// </summary>
    public TypeInfo? AssociatedType { get; set; }

    public TypedefSymbol (SymbolTable table, string name, string fullyQualifiedName)
        : base(table, SymbolKind.StructType, name, fullyQualifiedName)
    {}
}

public class FunctionOverload {
    /// <summary>
    /// The function symbol that contains this overload.
    /// </summary>
    [JsonIgnore]
    public FunctionSymbol Symbol { get; private init; }

    public List<TypeInfo> ParamTypes { get; private init; } = new();
    public TypeInfo? ReturnType { get; set; }
    public bool IsDuplicate { get; set; } = false;

    public FunctionOverload (FunctionSymbol symbol, List<TypeInfo> paramTypes) {
        Symbol = symbol;
        ParamTypes = paramTypes;
    }

    /// <summary>
    /// Returns true if the overload given matches the one in this symbol. If
    /// either overload contains unresolved types, this function returns false.
    /// </summary>
    /// <param name="paramTypes">The type of each parameter, in order.</param>
    public bool MatchesParamTypes (List<TypeInfo> paramTypes) {
        if (paramTypes.Count != ParamTypes.Count) return false;

        for (int i = 0; i < paramTypes.Count; i++) {
            if (paramTypes[i] == TypeInfo.UnresolvedType) return false;
            if (paramTypes[i] != ParamTypes[i]) return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if this function's overload types are all resolved.
    /// </summary>
    public bool IsResolved () {
        foreach (var type in ParamTypes) {
            if (type == TypeInfo.UnresolvedType) return false;
        }

        return true;
    }

    public string GetSignatureString () {
        //return Symbol.FullyQualifiedName + "("; 
        return "placeholder"; // TODO: Replace with something like "(std/Vec3;II)V".
    }
}