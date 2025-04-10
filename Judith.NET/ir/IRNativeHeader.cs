using Judith.NET.ir.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir;

public class IRNativeHeader : IRAssemblyHeader {
    public required TypeCollection TypeRefs { get; init; }
    public required FunctionCollection FuncRefs { get; init; }
    
    /// <summary>
    /// Maps the name of each type to its index in the JuVM.
    /// </summary>
    public required Dictionary<string, int> TypeIndices { get; init; }
    /// <summary>
    /// Maps the name of each function to its index in the JuVM.
    /// </summary>
    public required Dictionary<string, int> FunctionIndices { get; init; }

    private IRNativeHeader () {}

    public static IRNativeHeader Ver1 () {
        Dictionary<string, IRType> types = [];
        Dictionary<string, IRFunction> functions = [];

        TypeCollection typeRefs = new() {
            Void = AddType(new IRPseudoType("Void")),
            Any = AddType(new IRPseudoType("Any")),

            F64 = AddType(new IRPrimitiveType("F64")),
            I64 = AddType(new IRPrimitiveType("I64")),

            Bool = AddType(new IRPrimitiveType("Bool")),
            String = AddType(new IRPrimitiveType("String")),
        };

        FunctionCollection funcRefs = new() {
            Print = AddFunction(new IRFunction(
                "print",
                [
                    new IRParameter("value", typeRefs.String, true),
                ],
                typeRefs.Void,
                [],
                IRFunctionKind.Function,
                false
            )),
            Println = AddFunction(new IRFunction(
                "println",
                [
                    new IRParameter("value", typeRefs.String, true),
                ],
                typeRefs.Void,
                [],
                IRFunctionKind.Function,
                false
            )),
            Readln = AddFunction(new IRFunction(
                "readln",
                [],
                typeRefs.String,
                [],
                IRFunctionKind.Function,
                false
            )),
        };

        IRNativeHeader header = new() {
            Types = types,
            Functions = functions,
            TypeRefs = typeRefs,
            FuncRefs = funcRefs,
            TypeIndices = new() {
                [typeRefs.F64.Name] = 1,
                [typeRefs.I64.Name] = 2,
                [typeRefs.Bool.Name] = 3,
                [typeRefs.String.Name] = 4,
            },
            FunctionIndices = new() {
                [funcRefs.Print.Name] = 1,
                [funcRefs.Println.Name] = 2,
                [funcRefs.Readln.Name] = 3,
            },
        };

        return header;

        T AddType<T> (T irType) where T : IRType {
            types[irType.Name] = irType;
            return irType;
        }

        T AddFunction<T> (T irFunc) where T : IRFunction {
            functions[irFunc.Name] = irFunc;
            return irFunc;
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
        public required IRType Void;
        public required IRType Any;

        public required IRPrimitiveType F64;
        public required IRPrimitiveType I64;

        public required IRPrimitiveType Bool;
        public required IRPrimitiveType String;
    }

    public class FunctionCollection {
        public required IRFunction Print;
        public required IRFunction Println;
        public required IRFunction Readln;
    }
}
