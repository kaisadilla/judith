using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.codegen.jasm;

/// <summary>
/// The type of a constant from the constant table. This is unrelated to Judith
/// types, but instead refers to the different types of data the VM can handle.
/// </summary>
public enum JasmConstantType : byte {
    Error = 0,
    Int64,
    Float64,
    UnsignedInt64,
    StringUtf8,
    Bool,
}