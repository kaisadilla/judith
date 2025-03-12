using Judith.NET.analysis.semantics;
using Judith.NET.ir;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

public class JudithNativeHeader : IJudithHeader {
    public string Name { get; } = string.Empty;

    public Dictionary<string, TypeSymbol> Types { get; } = [];
    public Dictionary<string, FunctionSymbol> Functions { get; } = [];

    /// <summary>
    /// An object that contains the symbols for all the native types.
    /// </summary>
    public TypeCollection TypeRefs { get; private set; } = null!;

    public JudithNativeHeader (IRNativeHeader ir, TypeSymbol noType) {
        TypeRefs = new() {
            F64 = AddType(SymbolKind.PrimitiveType, ir.TypeRefs.F64.Name, noType),
            I64 = AddType(SymbolKind.PrimitiveType, ir.TypeRefs.I64.Name, noType),
            Bool = AddType(SymbolKind.PrimitiveType, ir.TypeRefs.Bool.Name, noType),
            String = AddType(SymbolKind.StringType, ir.TypeRefs.String.Name, noType),
        };
    }

    private TypeSymbol AddType (SymbolKind kind, string name, TypeSymbol noType) {
        var symbol = new TypeSymbol(kind, name, name, "") {
            Type = noType,
        };
        Types[symbol.FullyQualifiedName] = symbol;

        return symbol;
    }

    public class TypeCollection {
        public required TypeSymbol F64 { get; init; }
        public required TypeSymbol I64 { get; init; }
        public required TypeSymbol Bool { get; init; }
        public required TypeSymbol String { get; init; }
    }
}