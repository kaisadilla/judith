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
            case OpCode.Constant:
                return ConstantInstruction("CONSTANT", index);
            case OpCode.ConstantLong:
                return ConstantLongInstruction("CONSTANT_LONG", index);
            case OpCode.Return:
                return SimpleInstruction("RETURN", index);
            case OpCode.Negate:
                return SimpleInstruction("NEGATE", index);
            case OpCode.Add:
                return SimpleInstruction("ADD", index);
            case OpCode.Subtract:
                return SimpleInstruction("SUBSTRACT", index);
            case OpCode.Multiply:
                return SimpleInstruction("MULTIPLY", index);
            case OpCode.Divide:
                return SimpleInstruction("DIVIDE", index);
            case OpCode.CheckedAdd:
                break;
            case OpCode.CheckedSubtract:
                break;
            case OpCode.CheckedMultiply:
                break;
            case OpCode.CheckedDivide:
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
