using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

public class NativeFeatures {
    public TypeCollection Types { get; private set; }

    /// <summary>
    /// Adds all native types to the type and symbol tables given.
    /// </summary>
    public NativeFeatures (TypeTable typeTbl, SymbolTable symbolTbl) {
        Types = new() {
            F32 = AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "F32"),
            F64 = AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "F64"),

            I8 = AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "I8"),
            I16 = AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "I16"),
            I32 = AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "I32"),
            I64 = AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "I64"),

            Ui8 = AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "Ui8"),
            Ui16 = AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "Ui16"),
            Ui32 = AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "Ui32"),
            Ui64 = AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "Ui64"),

            String = AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "String"),
            Bool = AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "Bool"),

            Byte = AddNativeType(typeTbl, symbolTbl, SymbolKind.AliasType, "Byte"),
            Int = AddNativeType(typeTbl, symbolTbl, SymbolKind.AliasType, "Int"),
            Float = AddNativeType(typeTbl, symbolTbl, SymbolKind.AliasType, "Float"),
            Num = AddNativeType(typeTbl, symbolTbl, SymbolKind.AliasType, "Num"),

            Void = AddNativeType(typeTbl, symbolTbl, SymbolKind.NativeType, "Void"),
        };
    }

    public bool IsNumericType (TypeInfo typeInfo) {
        return typeInfo == Types.F32
            || typeInfo == Types.F64
            || typeInfo == Types.I8
            || typeInfo == Types.I16
            || typeInfo == Types.I32
            || typeInfo == Types.I64
            || typeInfo == Types.Ui8
            || typeInfo == Types.Ui16
            || typeInfo == Types.Ui32
            || typeInfo == Types.Ui64
            || typeInfo == Types.Byte
            || typeInfo == Types.Int
            || typeInfo == Types.Float
            || typeInfo == Types.Num;
    }

    /// <summary>
    /// Given two native numeric types, returns the type they coalesce to when
    /// operated, or <see langword="null"/> if they are incompatible, following
    /// this logic:
    /// - same type (float, int, uint): Will return the same type.
    /// - int and uint (or vice versa): null.
    /// - different type: Will return Num.
    /// - same size: Will return the same size.
    /// - different size: bigger size.
    /// </summary>
    public TypeInfo? CoalesceNumericTypes (TypeInfo a, TypeInfo b) {
        NumberType aType = _GetNumberType(a);
        NumberType bType = _GetNumberType(b);
        int aSize = _GetSize(a);
        int bSize = _GetSize(b);

        if (aType == NumberType.Integer && bType == NumberType.UnsignedInteger) {
            return null;
        }
        if (aType == NumberType.UnsignedInteger && bType == NumberType.Integer) {
            return null;
        }

        if (aType == bType) {
            return aSize > bSize ? a : b;
        }
        else {
            return Types.Num;
        }


        NumberType _GetNumberType (TypeInfo t) {
            if (t == Types.F32 || t == Types.F64 || t == Types.Float || t == Types.Num) {
                return NumberType.Float;
            }
            if (t == Types.I8 || t == Types.I16 || t == Types.I32 || t == Types.I16 || t == Types.Int) {
                return NumberType.Integer;
            }
            if (t == Types.Ui8 || t == Types.Ui16 || t == Types.Ui32 || t == Types.Ui16 || t == Types.Byte) {
                return NumberType.UnsignedInteger;
            }
            return NumberType.Float;
        }

        int _GetSize (TypeInfo t) {
            if (t == Types.I8 || t == Types.Ui8 || t == Types.Byte) return 8;
            if (t == Types.I16 || t == Types.Ui16) return 16;
            if (t == Types.F32 || t == Types.I32 || t == Types.Ui32) return 32;

            return 64;
        }
    }

    private static TypeInfo AddNativeType (
        TypeTable typeTbl, SymbolTable symbolTbl, SymbolKind symbolKind, string name
    ) {
        Symbol symbol = symbolTbl.AddSymbol(symbolKind, name);
        TypeInfo typeInfo = new(name, symbol.FullyQualifiedName);
        typeTbl.AddType(typeInfo);

        return typeInfo;
    }

    public class TypeCollection {
        public TypeInfo F32 { get; init; } = TypeInfo.UnresolvedType;
        public TypeInfo F64 { get; init; } = TypeInfo.UnresolvedType;

        public TypeInfo I8 { get; init; } = TypeInfo.UnresolvedType;
        public TypeInfo I16 { get; init; } = TypeInfo.UnresolvedType;
        public TypeInfo I32 { get; init; } = TypeInfo.UnresolvedType;
        public TypeInfo I64 { get; init; } = TypeInfo.UnresolvedType;

        public TypeInfo Ui8 { get; init; } = TypeInfo.UnresolvedType;
        public TypeInfo Ui16 { get; init; }= TypeInfo.UnresolvedType;
        public TypeInfo Ui32 { get; init; }= TypeInfo.UnresolvedType;
        public TypeInfo Ui64 { get; init; } = TypeInfo.UnresolvedType;

        public TypeInfo String { get; init; } = TypeInfo.UnresolvedType;
        public TypeInfo Bool { get; init; } = TypeInfo.UnresolvedType;

        public TypeInfo Byte { get; init; } = TypeInfo.UnresolvedType;
        public TypeInfo Int { get; init; } = TypeInfo.UnresolvedType;
        public TypeInfo Float { get; init; } = TypeInfo.UnresolvedType;
        public TypeInfo Num { get; init; } = TypeInfo.UnresolvedType;

        public TypeInfo Void { get; init; } = TypeInfo.UnresolvedType;
    }
}