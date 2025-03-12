using Judith.NET.ir.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir;

public class IRNativeHeader {
    public Dictionary<string, IRType> Types;

    public TypeCollection TypeRefs;

    private IRNativeHeader (Dictionary<string, IRType> types, TypeCollection typeRefs) {
        Types = types;
        TypeRefs = typeRefs;
    }

    public static IRNativeHeader Ver1 () {
        Dictionary<string, IRType> types = new();

        TypeCollection typeRefs = new() {
            F64 = AddType(new IRPrimitiveType("F64")),
            I64 = AddType(new IRPrimitiveType("I64")),

            Bool = AddType(new IRPrimitiveType("Bool")),
            String = AddType(new IRStringType("String")),
        };

        return new(types, typeRefs);

        T AddType<T> (T irType) where T : IRType {
            types[irType.Name] = irType;
            return irType;
        }
    }

    public class TypeCollection {
        public required IRPrimitiveType F64;
        public required IRPrimitiveType I64;

        public required IRPrimitiveType Bool;
        public required IRStringType String;
    }
}
