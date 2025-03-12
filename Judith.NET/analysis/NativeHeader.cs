using Judith.NET.analysis.semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

public class NativeHeader : IAssemblyHeader {
    public string Name { get; } = string.Empty;

    public Dictionary<string, Symbol> Symbols { get; } = [];

    /// <summary>
    /// An object that contains the symbols for all the native types.
    /// </summary>
    public TypeCollection Types { get; private set; } = null!;


    public static NativeHeader Ver1 () {
        var nc = new NativeHeader();
        nc.Types = new() {
            Unresolved = nc.AddPseudoType(SymbolKind.UnresolvedType, "!Unresolved"),

            Error = nc.AddPseudoType(SymbolKind.ErrorType, "<error-type"),

            NoType = nc.AddPseudoType(SymbolKind.PseudoType, "<no-type>"),
            Anonymous = nc.AddPseudoType(SymbolKind.PseudoType, "<anonymous-object>"),
            Void = nc.AddType(SymbolKind.PseudoType, "Void"),

            F32 = nc.AddType(SymbolKind.PrimitiveType, "F32"),
            F64 = nc.AddType(SymbolKind.PrimitiveType, "F64"),

            I8 = nc.AddType(SymbolKind.PrimitiveType, "I8"),
            I16 = nc.AddType(SymbolKind.PrimitiveType, "I16"),
            I32 = nc.AddType(SymbolKind.PrimitiveType, "I32"),
            I64 = nc.AddType(SymbolKind.PrimitiveType, "I64"),

            Ui8 = nc.AddType(SymbolKind.PrimitiveType, "Ui8"),
            Ui16 = nc.AddType(SymbolKind.PrimitiveType, "Ui16"),
            Ui32 = nc.AddType(SymbolKind.PrimitiveType, "Ui32"),
            Ui64 = nc.AddType(SymbolKind.PrimitiveType, "Ui64"),

            Bool = nc.AddType(SymbolKind.PrimitiveType, "Bool"),
            String = nc.AddType(SymbolKind.StringType, "String"),
            Char = nc.AddType(SymbolKind.CharType, "Char"),

            Byte = nc.AddType(SymbolKind.AliasType, "Byte"),
            Int = nc.AddType(SymbolKind.AliasType, "Int"),
            Float = nc.AddType(SymbolKind.AliasType, "Float"),
            Num = nc.AddType(SymbolKind.AliasType, "Num"),

            Function = nc.AddType(SymbolKind.FunctionType, "Function"),
        };
        nc.Types.Init();

        return nc;
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

    private TypeSymbol AddPseudoType (SymbolKind kind, string name) {
        return new TypeSymbol(kind, name, name, "");
    }

    private TypeSymbol AddType (SymbolKind kind, string name) {
        var symbol = new TypeSymbol(kind, name, name, "");
        Symbols[symbol.FullyQualifiedName] = symbol;

        return symbol;
    }

    public class TypeCollection {
        // Warning supressed as creating a constructor for this would be horrible.
        // Unresolved types:
        public required TypeSymbol Unresolved { get; init; }

        // Error types:
        public required TypeSymbol Error { get; init; }

        // Pseudotypes:
        public required TypeSymbol NoType { get; init; }
        public required TypeSymbol Anonymous { get; init; }
        public required TypeSymbol Void { get; init; }

        // Floating-point types:
        public required TypeSymbol F32 { get; init; }
        public required TypeSymbol F64 { get; init; }

        // Signed integer types:
        public required TypeSymbol I8 { get; init; }
        public required TypeSymbol I16 { get; init; }
        public required TypeSymbol I32 { get; init; }
        public required TypeSymbol I64 { get; init; }

        // Unsigned integer types:
        public required TypeSymbol Ui8 { get; init; }
        public required TypeSymbol Ui16 { get; init; }
        public required TypeSymbol Ui32 { get; init; }
        public required TypeSymbol Ui64 { get; init; }

        // Other native types:
        public required TypeSymbol Bool { get; init; }
        public required TypeSymbol String { get; init; }
        public required TypeSymbol Char { get; init; }

        // Default aliased types:
        public required TypeSymbol Byte { get; init; } // Default: Ui8
        public required TypeSymbol Int { get; init; } // Default: I64
        public required TypeSymbol Float { get; init; } // Default: F64
        public required TypeSymbol Num { get; init; } // Default: Float

        public required TypeSymbol Function { get; init; } // TODO: Turn into FuncTypeSymbol

        public void Init () {
            Unresolved.Type = NoType;

            Error.Type = NoType;

            NoType.Type = NoType;
            Anonymous.Type = NoType;
            Void.Type = NoType;

            F32.Type = NoType;
            F64.Type = NoType;

            I8.Type = NoType;
            I16.Type = NoType;
            I32.Type = NoType;
            I64.Type = NoType;

            Ui8.Type = NoType;
            Ui16.Type = NoType;
            Ui32.Type = NoType;
            Ui64.Type = NoType;

            Bool.Type = NoType;
            String.Type = NoType;
            Char.Type = NoType;

            Byte.Type = NoType;
            Int.Type = NoType;
            Float.Type = NoType;
            Num.Type = NoType;

            Function.Type = NoType;
        }
    }
}