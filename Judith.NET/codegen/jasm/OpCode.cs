using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.codegen.jasm;

public enum OpCode : byte {
    /// <summary>
    /// NOOP | NO_OPERATION | The no operation. Does nothing.
    /// </summary>
    NOOP = 0,

    /// <summary>
    /// NATIVE | NATIVE | Calls a VM function at the given address. This
    /// instruction can only be created by the VM itself.
    /// </summary>
    NATIVE,

    /// <summary>
    /// CONST | CONSTANT | Pushes the value $a as it appears in byte code.
    /// </summary>
    CONST,
    /// <summary>
    /// CONST_L | CONSTANT_LONG | Pushes the value $a as it appears in byte code.
    /// </summary>
    CONST_L,
    /// <summary>
    /// CONST_LL | CONSTANT_LONG_LONG | Pushes the value $a as it appears in
    /// byte code.
    /// </summary>
    CONST_LL,
    /// <summary>
    /// CONST_0 | CONSTANT_0 | Pushes the value "0".
    /// </summary>
    CONST_0,
    /// <summary>
    /// F_CONST_1 | F_CONSTANT_1 | Pushes the value "1" as f64.
    /// </summary>
    F_CONST_1,
    /// <summary>
    /// F_CONST_2 | F_CONSTANT_2 | Pushes the value "2" as f64.
    /// </summary>
    F_CONST_2,
    /// <summary>
    /// I_CONST_1 | I_CONSTANT_1 | Pushes the value "1" as i64.
    /// </summary>
    I_CONST_1,
    /// <summary>
    /// I_CONST_2 | I_CONSTANT_2 | Pushes the value "2" as i64.
    /// </summary>
    I_CONST_2,
    /// <summary>
    /// STR_CONST | STRING_CONSTANT | Pushes the value at $a index from the
    /// constant table as a string.
    /// </summary>
    STR_CONST,
    /// <summary>
    /// STR_CONST_L | STRING_CONSTANT_L | Pushes the value at $a index from
    /// the constant table as a string.
    /// </summary>
    STR_CONST_L,

    /// <summary>
    /// RET | RETURN
    /// </summary>
    RET,

    /// <summary>
    /// F_NEG | FLOAT_NEGATE | Negates the value (as f64) at the top of the stack.
    /// </summary>
    F_NEG,
    /// <summary>
    /// F_ADD | FLOAT_ADD | Adds top two values as f64.
    /// </summary>
    F_ADD,
    /// <summary>
    /// F_SUB | FLOAT_SUBTRACT | Substract top from top-1 as f64.
    /// </summary>
    F_SUB,
    /// <summary>
    /// F_MUL | FLOAT_MULTIPLY | Multiply top-1 by top as f64.
    /// </summary>
    F_MUL,
    /// <summary>
    /// F_DIV | FLOAT_DIVIDE | Divide top-1 by top as f64.
    /// </summary>
    F_DIV,
    /// <summary>
    /// F_GT | F_GREATER | Push 1 if top-1 &gt; top as f64.
    /// </summary>
    F_GT,
    /// <summary>
    /// F_GE | F_GREATER_EQUAL | Push 1 if top-1 &gt;= top as f64.
    /// </summary>
    F_GE,
    /// <summary>
    /// F_LT | F_LESS | Push 1 if top-1 &lt; top as f64.
    /// </summary>
    F_LT,
    /// <summary>
    /// F_LE | F_LESS_EQUAL | Push 1 if top-1 &lt;= top as f64.
    /// </summary>
    F_LE,

    /// <summary>
    /// I_NEG | INTEGER_NEGATE | Negates the value (as i64) at the top of the stack.
    /// </summary>
    I_NEG,
    I_ADD,
    I_ADD_CHECKED,
    I_SUB,
    I_SUB_CHECKED,
    I_MUL,
    I_MUL_CHECKED,
    I_DIV,
    I_DIV_CHECKED,
    I_GT,
    I_GE,
    I_LT,
    I_LE,

    /// <summary>
    /// EQ | EQUAL | Push 1 if top two values are equal, or 0 if not.
    /// </summary>
    EQ,
    /// <summary>
    /// NEQ | NOT_EQUAL | Push 0 if top two values are equal, or 1 if not.
    /// </summary>
    NEQ,

    STORE_0,
    STORE_1,
    STORE_2,
    STORE_3,
    STORE_4,
    STORE,
    STORE_L,

    LOAD_0,
    LOAD_1,
    LOAD_2,
    LOAD_3,
    LOAD_4,
    LOAD,
    LOAD_L,

    POP,

    /// <summary>
    /// JMP | JUMP | Jump to $a.
    /// </summary>
    JMP,
    /// <summary>
    /// JMP_L | JUMP_LONG | Jump to $a.
    /// </summary>
    JMP_L,
    /// <summary>
    /// JTRUE | JUMP_TRUE | Jump to $a if top != 0. Consume top.
    /// </summary>
    JTRUE,
    /// <summary>
    /// JTRUE_L | JUMP_TRUE_LONG | Jump to $a if top != 0. Consume top.
    /// </summary>
    JTRUE_L,
    /// <summary>
    /// JTRUE_K | JUMP_TRUE_KEEPIF | Jump to $a offset if top != 0.
    /// </summary>
    JTRUE_K,
    /// <summary>
    /// JTRUE_K_L | JUMP_TRUE_KEEPIF_LONG | Jump to $a offset if top != 0, consume otherwise.
    /// </summary>
    JTRUE_K_L,
    /// <summary>
    /// JFALSE | JUMP_FALSE | Jump to $a if top != 0. Consume top, consume otherwise.
    /// </summary>
    JFALSE,
    /// <summary>
    /// JFALSE_L | JUMP_FALSE_LONG | Jump to $a if top != 0. Consume top.
    /// </summary>
    JFALSE_L,
    /// <summary>
    /// JFALSE_K | JUMP_FALSE_KEEPIF | Jump to $a offset if top != 0, consume otherwise.
    /// </summary>
    JFALSE_K,
    /// <summary>
    /// JFALSE_K_L | JUMP_FALSE_KEEPIF_LONG | Jump to $a offset if top != 0, consume otherwise.
    /// </summary>
    JFALSE_K_L,

    /// <summary>
    /// CALL | CALL | Call function at FunctionRef index $a.
    /// </summary>
    CALL,

    PRINT,
}