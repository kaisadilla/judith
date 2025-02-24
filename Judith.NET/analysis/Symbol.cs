using Judith.NET.diagnostics.serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
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
    Function,
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
    public List<TypeInfo> Overload { get; private init; } = new();
    public TypeInfo? ReturnType { get; set; }

    public FunctionSymbol (
        SymbolTable table,
        string name,
        string fullyQualifiedName,
        List<TypeInfo> overload,
        TypeInfo? returnType
    )
        : base(table, SymbolKind.Function, name, fullyQualifiedName)
    {
        Overload = overload;
        ReturnType = returnType;
    }

    /// <summary>
    /// Returns true if the overload given matches the one in this symbol. If
    /// either overload contains unresolved types, 
    /// </summary>
    /// <param name="overload">The type of each parameter, in order.</param>
    public bool MatchesOverload (List<TypeInfo> overload) {
        for (int i = 0; i < overload.Count; i++) {
            if (overload[i] == TypeInfo.UnresolvedType) return false;
            if (overload[i] != Overload[i]) return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if this function's overload types are all resolved.
    /// </summary>
    public bool IsResolved () {
        foreach (var type in Overload) {
            if (type == TypeInfo.UnresolvedType) return false;
        }

        return true;
    }
}