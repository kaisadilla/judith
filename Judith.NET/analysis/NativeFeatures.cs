using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

public static class NativeFeatures {
    /// <summary>
    /// Adds all native types to the type and symbol tables given.
    /// </summary>
    public static void AddNativeTypes (TypeTable typeTbl, SymbolTable symbolTbl) {
        AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "F32");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "F64");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "I8");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "I16");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "I32");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "I64");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "Ui8");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "Ui16");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "Ui32");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "Ui64");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "String");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "Bool");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.AliasType, "Int");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.AliasType, "Float");
        AddNativeType(typeTbl, symbolTbl, SymbolKind.AliasType, "Num");
    }

    private static void AddNativeType (
        TypeTable typeTbl, SymbolTable symbolTbl, SymbolKind symbolKind, string name
    ) {
        Symbol symbol = symbolTbl.AddSymbol(symbolKind, name);
        typeTbl.AddType(new(name, symbol.FullyQualifiedName));
    }
}
