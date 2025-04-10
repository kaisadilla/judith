using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public enum IRTypeKind {
    Regular,
    Box,
    Ptr,
    GcPtr,
    UniquePtr,
    SharedPtr,
}

public struct IRTypeName {
    public IRTypeKind Kind { get; }
    public string Name { get; }

    public IRTypeName (IRTypeKind kind, string name) {
        Kind = kind;
        Name = name;
    }
}
