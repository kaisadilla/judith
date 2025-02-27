using Judith.NET.analysis.semantics;
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
    public NativeFeatures (SymbolTable tbl) {
        Types = new() {
            Unresolved = AddType(tbl, SymbolKind.UnresolvedType, "!Unresolved"),
            UnresolvedFunction = AddType(tbl, SymbolKind.UnresolvedType, "!Function"),

            Error = AddType(tbl, SymbolKind.ErrorType, "<error-type>"),

            NoType = AddType(tbl, SymbolKind.PseudoType, "<no-type>"),
            Anonymous = AddType(tbl, SymbolKind.PseudoType, "<anonymous-object>"),
            Void = AddType(tbl, SymbolKind.PseudoType, "Void"),

            F32 = AddType(tbl, SymbolKind.PrimitiveType, "F32"),
            F64 = AddType(tbl, SymbolKind.PrimitiveType, "F64"),

            I8 = AddType(tbl, SymbolKind.PrimitiveType, "I8"),
            I16 = AddType(tbl, SymbolKind.PrimitiveType, "I16"),
            I32 = AddType(tbl, SymbolKind.PrimitiveType, "I32"),
            I64 = AddType(tbl, SymbolKind.PrimitiveType, "I64"),

            Ui8 = AddType(tbl, SymbolKind.PrimitiveType, "Ui8"),
            Ui16 = AddType(tbl, SymbolKind.PrimitiveType, "Ui16"),
            Ui32 = AddType(tbl, SymbolKind.PrimitiveType, "Ui32"),
            Ui64 = AddType(tbl, SymbolKind.PrimitiveType, "Ui64"),

            Bool = AddType(tbl, SymbolKind.PrimitiveType, "Bool"),
            String = AddType(tbl, SymbolKind.StringType, "String"),

            Byte = AddType(tbl, SymbolKind.AliasType, "Byte"),
            Int = AddType(tbl, SymbolKind.AliasType, "Int"),
            Float = AddType(tbl, SymbolKind.AliasType, "Float"),
            Num = AddType(tbl, SymbolKind.AliasType, "Num"),
        };
    }

    public bool IsNumericType (TypeSymbol type) {
        return type == Types.F32
            || type == Types.F64
            || type == Types.I8
            || type == Types.I16
            || type == Types.I32
            || type == Types.I64
            || type == Types.Ui8
            || type == Types.Ui16
            || type == Types.Ui32
            || type == Types.Ui64
            || type == Types.Byte
            || type == Types.Int
            || type == Types.Float
            || type == Types.Num;
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
    public TypeSymbol? CoalesceNumericTypes (TypeSymbol a, TypeSymbol b) {
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


        NumberType _GetNumberType (TypeSymbol t) {
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

        int _GetSize (TypeSymbol t) {
            if (t == Types.I8 || t == Types.Ui8 || t == Types.Byte) return 8;
            if (t == Types.I16 || t == Types.Ui16) return 16;
            if (t == Types.F32 || t == Types.I32 || t == Types.Ui32) return 32;

            return 64;
        }
    }

    private TypeSymbol AddType (SymbolTable tbl, SymbolKind kind, string name) {
        return (TypeSymbol)tbl.AddSymbol(TypeSymbol.Define(kind, name));
    }

    public class TypeCollection {
        // Warning supressed as creating a constructor for this would be horrible.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        // Unresolved types:
        public TypeSymbol Unresolved { get; init; }
        public TypeSymbol UnresolvedFunction { get; init; }

        // Error types:
        public TypeSymbol Error { get; init; }

        // Pseudotypes:
        public TypeSymbol NoType { get; init; }
        public TypeSymbol Anonymous { get; init; }
        public TypeSymbol Void { get; init; }

        // Floating-point types:
        public TypeSymbol F32 { get; init; }
        public TypeSymbol F64 { get; init; }

        // Signed integer types:
        public TypeSymbol I8 { get; init; }
        public TypeSymbol I16 { get; init; }
        public TypeSymbol I32 { get; init; }
        public TypeSymbol I64 { get; init; }

        // Unsigned integer types:
        public TypeSymbol Ui8 { get; init; }
        public TypeSymbol Ui16 { get; init; }
        public TypeSymbol Ui32 { get; init; }
        public TypeSymbol Ui64 { get; init; }

        // Other native types:
        public TypeSymbol Bool { get; init; }
        public TypeSymbol String { get; init; }

        // Default aliased types:
        public TypeSymbol Byte { get; init; } // Default: Ui8
        public TypeSymbol Int { get; init; } // Default: I64
        public TypeSymbol Float { get; init; } // Default: F64
        public TypeSymbol Num { get; init; } // Default: Float
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}