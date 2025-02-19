using Judith.NET.compiler.jub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.diagnostics;

public class JasmDisassembler {
    private BinaryFile _file;
    private Chunk _chunk;

    public string Dump { get; private set; } = string.Empty;

    public JasmDisassembler (BinaryFile file, Chunk chunk) {
        _file = file;
        _chunk = chunk;
    }

    public void Disassemble () {
        Dump = string.Empty;

        int index = 0;
        while (index < _chunk.Code.Count) {
            index = DisassembleInstruction(index);
            Dump += "\n";
        }
    }

    private int DisassembleInstruction (int index) {
        OpCode opCode = (OpCode)_chunk.Code[index];
        Dump += $"Line {_chunk.Lines[index],-5} | {HexByteStr(index)} ";

        switch (opCode) {
            case OpCode.NoOp:
                return SimpleInstruction("NOOP", index);
            case OpCode.Const:
                return ConstantInstruction("CONST", index);
            case OpCode.ConstLong:
                return ConstantLongInstruction("CONST_L", index);
            case OpCode.Const0:
                return SimpleInstruction("CONST_0", index);
            case OpCode.IConst1:
                return SimpleInstruction("I_CONST_1", index);
            case OpCode.IConst2:
                return SimpleInstruction("I_CONST_2", index);
            case OpCode.ConstStr:
                return ConstantInstruction("CONST_STR", index);
            case OpCode.ConstStrLong:
                return ConstantLongInstruction("CONST_STR_L", index);

            case OpCode.Ret:
                return SimpleInstruction("RET", index);

            case OpCode.FNeg:
                return SimpleInstruction("F_NEG", index);
            case OpCode.FAdd:
                return SimpleInstruction("F_ADD", index);
            case OpCode.FSub:
                return SimpleInstruction("F_SUB", index);
            case OpCode.FMul:
                return SimpleInstruction("F_MUL", index);
            case OpCode.FDiv:
                return SimpleInstruction("F_DIV", index);
            case OpCode.FGt:
                return SimpleInstruction("F_GT", index);
            case OpCode.FGe:
                return SimpleInstruction("F_GE", index);
            case OpCode.FLt:
                return SimpleInstruction("F_LT", index);
            case OpCode.FLe:
                return SimpleInstruction("F_LE", index);

            case OpCode.INeg:
                return SimpleInstruction("I_NEG", index);
            case OpCode.IAdd:
                return SimpleInstruction("I_ADD", index);
            case OpCode.IAddChecked:
                return SimpleInstruction("I_ADD_CHECKED", index);
            case OpCode.ISub:
                return SimpleInstruction("I_SUB", index);
            case OpCode.ISubChecked:
                return SimpleInstruction("I_SUB_CHECKED", index);
            case OpCode.IMul:
                return SimpleInstruction("I_MUL", index);
            case OpCode.IMulChecked:
                return SimpleInstruction("I_MUL_CHECKED", index);
            case OpCode.IDiv:
                return SimpleInstruction("I_DIV", index);
            case OpCode.IDivChecked:
                return SimpleInstruction("I_DIV_CHECKED", index);
            case OpCode.IGt:
                return SimpleInstruction("I_GT", index);
            case OpCode.IGe:
                return SimpleInstruction("I_GE", index);
            case OpCode.ILt:
                return SimpleInstruction("I_LT", index);
            case OpCode.ILe:
                return SimpleInstruction("I_LE", index);

            case OpCode.Eq:
                return SimpleInstruction("EQ", index);
            case OpCode.Neq:
                return SimpleInstruction("NEQ", index);

            case OpCode.Store0:
                return SimpleInstruction("STORE_0", index);
            case OpCode.Store1:
                return SimpleInstruction("STORE_1", index);
            case OpCode.Store2:
                return SimpleInstruction("STORE_2", index);
            case OpCode.Store3:
                return SimpleInstruction("STORE_3", index);
            case OpCode.Store4:
                return SimpleInstruction("STORE_4", index);
            case OpCode.Store:
                return ByteInstruction("STORE", index);
            case OpCode.StoreLong:
                return U16Instruction("STORE_L", index);

            case OpCode.Load0:
                return SimpleInstruction("LOAD_0", index);
            case OpCode.Load1:
                return SimpleInstruction("LOAD_1", index);
            case OpCode.Load2:
                return SimpleInstruction("LOAD_2", index);
            case OpCode.Load3:
                return SimpleInstruction("LOAD_3", index);
            case OpCode.Load4:
                return SimpleInstruction("LOAD_4", index);
            case OpCode.Load:
                return ByteInstruction("LOAD", index);
            case OpCode.LoadLong:
                return U16Instruction("LOAD_L", index);

            case OpCode.Jmp:
                return JumpInstruction("JMP", index);
            case OpCode.JmpLong:
                return JumpLongInstruction("JMP_L", index);
            case OpCode.JTrue:
                return JumpInstruction("JTRUE", index);
            case OpCode.JTrueLong:
                return JumpLongInstruction("JTRUE_L", index);
            case OpCode.JTrueK:
                return JumpInstruction("JTRUE_C", index);
            case OpCode.JTrueKLong:
                return JumpLongInstruction("JTRUE_C_L", index);
            case OpCode.JFalse:
                return JumpInstruction("JFALSE", index);
            case OpCode.JFalseLong:
                return JumpLongInstruction("JFALSE_L", index);
            case OpCode.JFalseK:
                return JumpInstruction("JFALSE_C", index);
            case OpCode.JFalseKLong:
                return JumpLongInstruction("JFALSE_C_L", index);

            case OpCode.Print:
                return PrintInstruction("PRINT", index);
            default:
                break;
        }

        return UnknownInstruction(index);
    }

    private int SimpleInstruction (string name, int index) {
        Dump += IdStr(name);
        return index + 1;
    }

    private int ByteInstruction (string name, int index) {
        var val = _chunk.Code[index + 1];

        Dump += IdStr(name) + " ";
        Dump += HexByteStr(val) + " ";

        return index + 2;
    }

    private int SByteInstruction (string name, int index) {
        sbyte val = unchecked((sbyte)_chunk.Code[index + 1]);

        Dump += IdStr(name) + " ";
        Dump += HexSByteStr(val) + " ";

        return index + 2;
    }

    private int U16Instruction (string name, int index) {
        var val = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8);

        Dump += IdStr(name) + " ";
        Dump += HexIntegerStr(val) + " ";

        return index + 3;
    }

    private int I32Instruction (string name, int index) {
        var val = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24);

        Dump += IdStr(name) + " ";
        Dump += HexIntegerStr(val) + " ";

        return index + 5;
    }

    private int ConstantInstruction (string name, int index) {
        var constIndex = _chunk.Code[index + 1];

        Dump += IdStr(name) + " ";
        Dump += HexByteStr(constIndex) + " ; " + Constant(constIndex);

        return index + 2;
    }

    private int ConstantLongInstruction (string name, int index) {
        var constIndex = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24);

        Dump += IdStr(name) + " ";
        Dump += HexIntegerStr(constIndex) + " ; " + Constant(constIndex);

        return index + 5;
    }

    private int JumpInstruction (string name, int index) {
        sbyte val = unchecked((sbyte)_chunk.Code[index + 1]);

        Dump += IdStr(name) + " ";
        Dump += HexSByteStr(val) + " ; to " + HexSByteStr(index + val + 2);

        return index + 2;
    }

    private int JumpLongInstruction (string name, int index) {
        var val = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24);

        Dump += IdStr(name) + " ";
        Dump += HexIntegerStr(val) + " ; to " + HexSByteStr(index + val + 5);

        return index + 5;
    }

    private int PrintInstruction (string name, int index) {
        var constType = _chunk.Code[index + 1];

        Dump += IdStr(name) + " ";
        Dump += HexByteStr(constType) + " ; " + (ConstantType)constType;

        return index + 2;
    }

    private int UnknownInstruction (int index) {
        Dump += $"0x{index:X4} <Unknown>";
        return index + 1;
    }

    private string Constant (int constIndex) {
        int offset = _file.ConstantTable.Offsets[constIndex];
        ConstantType ctype = (ConstantType)_file.ConstantTable.Bytes[offset++];

        switch (ctype) {
            case ConstantType.Error:
                return "<error-type>";
            case ConstantType.Int64:
                return _file.ConstantTable.ReadInt64(offset).ToString();
            case ConstantType.Float64:
                return _file.ConstantTable.ReadFloat64(offset).ToString();
            case ConstantType.UnsignedInt64:
                return _file.ConstantTable.ReadUnsignedInt64(offset).ToString();
            case ConstantType.StringASCII:
                return '"' + _file.ConstantTable.ReadStringASCII(offset) + '"';
            default:
                return "<unknown-type>";
        }
    }

    private static string HexByteStr (int index) => $"0x{index:X4}";
    private static string HexSByteStr (int index) => $"0x{index:X4}";
    private static string HexIntegerStr (long index) => $"0x{index:X8}";
    private static string JFloatStr (double val) => $"{val}";
    private static string IdStr (string id) => id.PadRight(16, ' ');
}
