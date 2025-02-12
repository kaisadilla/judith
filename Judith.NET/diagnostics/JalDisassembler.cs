using Judith.NET.compiler.jal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.diagnostics;

public class JalDisassembler {
    private JalChunk _chunk;

    public string Dump { get; private set; } = string.Empty;

    public JalDisassembler (JalChunk chunk) {
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

    private int ConstantInstruction (string name, int index) {
        var constIndex = _chunk.Code[index + 1];
        var constant = _chunk.Constants[constIndex];

        Dump += IdStr(name) + " ";
        Dump += HexByteStr(_chunk.Code[index + 1]) + " ";

        if (constant.Type == JalValueType.Float64 && constant is JalValue<double> c_f64) {
            Dump += $"; {JFloatStr(c_f64.Value)}";
        }
        else {
            Dump += "<unknown value type>";
        }

        return index + 2;
    }

    private int ConstantLongInstruction (string name, int index) {
        var constIndex = _chunk.Code[index + 1]
            + (_chunk.Code[index + 2] << 8)
            + (_chunk.Code[index + 3] << 16)
            + (_chunk.Code[index + 4] << 24);

        var constant = _chunk.Constants[constIndex];

        Dump += IdStr(name) + " ";
        Dump += HexIntegerStr(_chunk.Code[index + 1]) + " ";

        if (constant.Type == JalValueType.Float64 && constant is JalValue<double> c_f64) {
            Dump += $"; {JFloatStr(c_f64.Value)}";
        }
        else {
            Dump += $"<unknown value type>";
        }

        return index + 5;
    }

    private int UnknownInstruction (int index) {
        Dump += $"0x{index:X4} <Unknown>";
        return index + 1;
    }

    private static string HexByteStr (int index) => $"0x{index:X4}";
    private static string HexIntegerStr (long index) => $"0x{index:X8}";
    private static string JFloatStr (double val) => $"{val}";
    private static string IdStr (string id) => id.PadRight(16, ' ');
}
