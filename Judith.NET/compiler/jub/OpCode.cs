using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jub;

public enum OpCode : byte {
    /// <summary>
    /// NOOP | NO_OPERATION | The no operation. Does nothing.
    /// </summary>
    NoOp = 0,

    /// <summary>
    /// CONST | CONSTANT | Reads a constant at the address given by the next
    /// byte in the chunk. 
    /// </summary>
    Const,
    /// <summary>
    /// CONST_L | CONSTANT_LONG | Reads a constant at the address given in the
    /// next int32 in the chunk.
    /// </summary>
    ConstLong,
    /// <summary>
    /// CONST_0 | CONSTANT_0 | Pushes the value "0".
    /// </summary>
    Const0,
    /// <summary>
    /// CONST_1 | CONSTANT_1 | Pushes the value "1".
    /// </summary>
    IConst1,
    /// <summary>
    /// CONST_2 | CONSTANT_2 | Pushes the value "2".
    /// </summary>
    IConst2,
    /// <summary>
    /// CONST_STR | CONSTANT_STRING | Pushes the value at $a index from the
    /// constant table as a string.
    /// </summary>
    ConstStr,
    /// <summary>
    /// CONST_STR_L | CONSTANT_STRING_LONG | Pushes the value at $a index from
    /// the constant table as a string.
    /// </summary>
    ConstStrLong,

    /// <summary>
    /// RET | RETURN
    /// </summary>
    Ret,

    /// <summary>
    /// F_NEG | FLOAT_NEGATE | Negates the value (as f64) at the top of the stack.
    /// </summary>
    FNeg,
    /// <summary>
    /// F_ADD | FLOAT_ADD | Adds top two values as f64.
    /// </summary>
    FAdd,
    /// <summary>
    /// F_SUB | FLOAT_SUBTRACT | Substract top from top-1 as f64.
    /// </summary>
    FSub,
    /// <summary>
    /// F_MUL | FLOAT_MULTIPLY | Multiply top-1 by top as f64.
    /// </summary>
    FMul,
    /// <summary>
    /// F_DIV | FLOAT_DIVIDE | Divide top-1 by top as f64.
    /// </summary>
    FDiv,
    /// <summary>
    /// I_NEG | INTEGER_NEGATE | Negates the value (as i64) at the top of the stack.
    /// </summary>
    /// 
    INeg,
    IAdd,
    IAddChecked,
    ISub,
    ISubChecked,
    IMul,
    IMulChecked,
    IDiv,
    IDivChecked,

    Store0,
    Store1,
    Store2,
    Store3,
    Store4,
    Store,
    StoreLong,

    Load0,
    Load1,
    Load2,
    Load3,
    Load4,
    Load,
    LoadLong,

    Print,
}