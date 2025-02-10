using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jal;

public enum OpCode : byte {
    /// <summary>
    /// The no operation. Does nothing.
    /// </summary>
    NoOp = 0,
    /// <summary>
    /// Reads a constant at the address given by the next byte in the chunk. 
    /// </summary>
    Constant,
    /// <summary>
    /// Reads a constant at the address given in the next int32 in the chunk.
    /// </summary>
    ConstantLong,
    Return,
    /// <summary>
    /// Negates the current value.
    /// </summary>
    Negate,
    Add,
    Subtract,
    Multiply,
    Divide,
    CheckedAdd,
    CheckedSubtract,
    CheckedMultiply,
    CheckedDivide,

    Print,
}