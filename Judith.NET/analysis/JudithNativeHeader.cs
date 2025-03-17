using Judith.NET.analysis.semantics;
using Judith.NET.ir;
using Judith.NET.ir.syntax;
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
    /// Maps the name of each native function to its index.
    /// </summary>
    public Dictionary<string, int> FuncIndices { get; } = [];

    /// <summary>
    /// An object that contains the symbols for all the native types.
    /// </summary>
    public TypeCollection TypeRefs { get; private set; } = null!;

    public JudithNativeHeader (IRNativeHeader ir, JudithCompilation cmp) {
        TypeRefs = new() {
            Void = AddType(SymbolKind.PseudoType, ir.TypeRefs.Void.Name, cmp),
            F64 = AddType(SymbolKind.PrimitiveType, ir.TypeRefs.F64.Name, cmp),
            I64 = AddType(SymbolKind.PrimitiveType, ir.TypeRefs.I64.Name, cmp),
            Bool = AddType(SymbolKind.PrimitiveType, ir.TypeRefs.Bool.Name, cmp),
            String = AddType(SymbolKind.StringType, ir.TypeRefs.String.Name, cmp),
        };

        int index = 1; // Ignore Index # 0 - error func.
        foreach (var irFunc in ir.Functions.Values) {
            var func = CreateFunction(irFunc, cmp);
            FuncIndices[func.FullyQualifiedName] = index;
            index++;
        }
    }

    private TypeSymbol AddType (SymbolKind kind, string name, JudithCompilation cmp) {
        var symbol = new TypeSymbol(kind, name, name, "") {
            Type = cmp.PseudoTypes.NoType,
        };
        Types[symbol.FullyQualifiedName] = symbol;

        return symbol;
    }

    private FunctionSymbol CreateFunction (IRFunction irFunc, JudithCompilation cmp) {
        List<TypeSymbol> parameters = [];

        foreach (var param in irFunc.Parameters) {
            if (Types.TryGetValue(param.Type, out var paramType) == false) {
                throw new($"Native type '{param.Type}' does not exist.");
            }
            parameters.Add(paramType);
        }

        if (Types.TryGetValue(irFunc.ReturnType, out var returnType) == false) {
            throw new($"Native type '{irFunc.ReturnType}' does not exist.");
        }

        var symbol = new FunctionSymbol(parameters, irFunc.Name, irFunc.Name, "") {
            Type = cmp.PseudoTypes.Function,
            ReturnType = returnType,
        };
        Functions[symbol.FullyQualifiedName] = symbol;

        return symbol;
    }

    public class TypeCollection {
        public required TypeSymbol Void { get; init; }
        public required TypeSymbol F64 { get; init; }
        public required TypeSymbol I64 { get; init; }
        public required TypeSymbol Bool { get; init; }
        public required TypeSymbol String { get; init; }
    }
}