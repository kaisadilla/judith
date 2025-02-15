﻿using Judith.NET.compiler.jub;
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
        Dump += $"Line {_chunk.CodeLines[index],-5} | {HexByteStr(index)} ";

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
            case OpCode.INeg:
                break;
            case OpCode.IAdd:
                break;
            case OpCode.IAddChecked:
                break;
            case OpCode.ISub:
                break;
            case OpCode.ISubChecked:
                break;
            case OpCode.IMul:
                break;
            case OpCode.IMulChecked:
                break;
            case OpCode.IDiv:
                break;
            case OpCode.IDivChecked:
                break;

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

            case OpCode.Print:
                return SimpleInstruction("PRINT", index);
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

    private int U16Instruction (string name, int index) {
        var val = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8);

        Dump += IdStr(name) + " ";
        Dump += HexIntegerStr(val) + " ";

        return index + 3;
    }

    private int ConstantInstruction (string name, int index) {
        var constIndex = _chunk.Code[index + 1];

        Dump += IdStr(name) + " ";
        Dump += HexByteStr(_chunk.Code[index + 1]) + " ; " + Constant(constIndex);

        return index + 2;
    }

    private int ConstantLongInstruction (string name, int index) {
        var constIndex = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24);

        Dump += IdStr(name) + " ";
        Dump += HexIntegerStr(_chunk.Code[index + 1]) + " ; " + Constant(constIndex);

        return index + 5;
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
    private static string HexIntegerStr (long index) => $"0x{index:X8}";
    private static string JFloatStr (double val) => $"{val}";
    private static string IdStr (string id) => id.PadRight(16, ' ');
}
