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
    /// F_GT | F_GREATER | Push 1 if top-1 &gt; top as f64.
    /// </summary>
    FGt,
    /// <summary>
    /// F_GE | F_GREATER_EQUAL | Push 1 if top-1 &gt;= top as f64.
    /// </summary>
    FGe,
    /// <summary>
    /// F_LT | F_LESS | Push 1 if top-1 &lt; top as f64.
    /// </summary>
    FLt,
    /// <summary>
    /// F_LE | F_LESS_EQUAL | Push 1 if top-1 &lt;= top as f64.
    /// </summary>
    FLe,

    /// <summary>
    /// I_NEG | INTEGER_NEGATE | Negates the value (as i64) at the top of the stack.
    /// </summary>
    INeg,
    IAdd,
    IAddChecked,
    ISub,
    ISubChecked,
    IMul,
    IMulChecked,
    IDiv,
    IDivChecked,
    IGt,
    IGe,
    ILt,
    ILe,

    /// <summary>
    /// EQ | EQUAL | Push 1 if top two values are equal, or 0 if not.
    /// </summary>
    Eq,
    /// <summary>
    /// NEQ | NOT_EQUAL | Push 0 if top two values are equal, or 1 if not.
    /// </summary>
    Neq,

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

    /// <summary>
    /// JMP | JUMP | Jump to $a.
    /// </summary>
    Jmp,
    /// <summary>
    /// JMP_L | JUMP_LONG | Jump to $a.
    /// </summary>
    JmpLong,
    /// <summary>
    /// JTRUE | JUMP_TRUE | Jump to $a if top != 0. Consume top.
    /// </summary>
    JTrue,
    /// <summary>
    /// JTRUE_L | JUMP_TRUE_LONG | Jump to $a if top != 0. Consume top.
    /// </summary>
    JTrueLong,
    /// <summary>
    /// JTRUE_C | JUMP_TRUE_KEEPIF | Jump to $a offset if top != 0.
    /// </summary>
    JTrueK,
    /// <summary>
    /// JTRUE_C_L | JUMP_TRUE_KEEPIF_LONG | Jump to $a offset if top != 0, consume otherwise.
    /// </summary>
    JTrueKLong,
    /// <summary>
    /// JFALSE | JUMP_FALSE | Jump to $a if top != 0. Consume top, consume otherwise.
    /// </summary>
    JFalse,
    /// <summary>
    /// JFALSE_L | JUMP_FALSE_LONG | Jump to $a if top != 0. Consume top.
    /// </summary>
    JFalseLong,
    /// <summary>
    /// JFALSE_C | JUMP_FALSE_KEEPIF | Jump to $a offset if top != 0, consume otherwise.
    /// </summary>
    JFalseK,
    /// <summary>
    /// JFALSE_C_L | JUMP_FALSE_KEEPIF_LONG | Jump to $a offset if top != 0, consume otherwise.
    /// </summary>
    JFalseKLong,

    /// <summary>
    /// CALL | CALL | Call function at FunctionRef index $a.
    /// </summary>
    Call,

    Print,
    InternalFunc,
}