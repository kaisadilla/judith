using Judith.NET.ir.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir;

public class IRNativeHeader : IRAssemblyHeader {
    public required TypeCollection TypeRefs { get; init; }

    /// <summary>
    /// Maps the name of each type to its index in the JuVM.
    /// </summary>
    public required Dictionary<string, int> TypeIndices { get; init; }
    public required Dictionary<string, int> FunctionIndices { get; init; }

    private IRNativeHeader () {}

    public static IRNativeHeader Ver1 () {
        Dictionary<string, IRType> types = new();

        TypeCollection typeRefs = new() {
            F64 = AddType(new IRPrimitiveType("F64")),
            I64 = AddType(new IRPrimitiveType("I64")),

            Bool = AddType(new IRPrimitiveType("Bool")),
            String = AddType(new IRStringType("String")),
        };

        IRNativeHeader header = new() {
            Types = types,
            Functions = [],
            TypeRefs = typeRefs,
            TypeIndices = new() {
                [typeRefs.F64.Name] = 1,
                [typeRefs.I64.Name] = 2,
                [typeRefs.Bool.Name] = 3,
                [typeRefs.String.Name] = 4,
            },
            FunctionIndices = new() {

            },
        };

        return header;

        T AddType<T> (T irType) where T : IRType {
            types[irType.Name] = irType;
            return irType;
        }
    }

    /// <summary>
    /// Returns the index of the type with the name given, if it's part of the
    /// native assembly.
    /// </summary>
    /// <param name="name">The type's name.</param>
    /// <param name="index">Its index in the native assembly.</param>
    public bool TryGetTypeIndex (string name, out int index) {
        return TypeIndices.TryGetValue(name, out index);
    }

    /// <summary>
    /// Returns the index of the function with the name given, if it's part of
    /// the native assembly.
    /// </summary>
    /// <param name="name">The function's name.</param>
    /// <param name="index">Its index in the native assembly.</param>
    public bool TryGetFunctionIndex (string name, out int index) {
        return FunctionIndices.TryGetValue(name, out index);
    }

    public class TypeCollection {
        public required IRPrimitiveType F64;
        public required IRPrimitiveType I64;

        public required IRPrimitiveType Bool;
        public required IRStringType String;
    }
}
