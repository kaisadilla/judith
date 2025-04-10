using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public abstract class IRType : IRNode {
    public string Name { get; private init; }

    protected IRType (string name) {
        Name = name;
    }
}

public class IRPseudoType : IRType {
    public IRPseudoType (string name) : base(name) { }
}

public class IRPrimitiveType : IRType {
    public IRPrimitiveType (string name) : base(name) { }
}

public class IRUserType : IRType {
    public IRUserType (string name) : base(name) { }
}

public class IRBoxType : IRType {
    public IRType BoxedType { get; private init; }

    public IRBoxType (IRType boxedType) : base("Box") {
        BoxedType = boxedType;
    }
}

public class IRPointerType : IRType {
    public IRType PointedType { get; private init; }

    public IRPointerType (IRType pointedType) : base("Ptr") {
        PointedType = pointedType;
    }
}

public class IRGcPointerType : IRType {
    public IRType PointedType { get; private init; }

    public IRGcPointerType (IRType pointedType) : base("GcPtr") {
        PointedType = pointedType;
    }
}

public class IRUniquePointerType : IRType {
    public IRType PointedType { get; private init; }

    public IRUniquePointerType (IRType pointedType) : base("UniquePtr") {
        PointedType = pointedType;
    }
}

public class IRSharedPointerType : IRType {
    public IRType PointedType { get; private init; }

    public IRSharedPointerType (IRType pointedType) : base("SharedPtr") {
        PointedType = pointedType;
    }
}