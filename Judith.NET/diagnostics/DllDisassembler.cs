using Judith.NET.codegen.jasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.diagnostics;

public class DllDisassembler {
    private JasmAssembly _dll;

    public string ChunkString { get; private set; } = string.Empty;

    private StringTable _stringTable = null!;
    private Chunk _chunk = null!;

    private static string[] _sizeUnits = ["KiB", "MiB", "GiB", "TiB"];

    public DllDisassembler (JasmAssembly dll) {
        _dll = dll;
    }

    public void Disassemble () {
        ChunkString = string.Empty;

        Console.WriteLine("===== Function reference table =====");
        Console.WriteLine("ADDRESS | Block index | Func index   ");
        for (int i = 0; i < _dll.FunctionRefTable.Size; i++) {
            var fr = _dll.FunctionRefTable[i];
            Console.WriteLine($" 0x{i,4:X4} |    {fr.Block,8} |    {fr.Index,8 }");
        }

        Console.WriteLine("");
        Console.WriteLine("===== Blocks =====");

        for (int i = 0; i < _dll.Blocks.Count; i++) {
            DisassembleBlock(_dll.Blocks[i], i);
        }
    }

    public void DisassembleBlock (BinaryBlock block, int blockIndex) {
        Console.WriteLine($"==== Block #0x{blockIndex,4:X4} ====");
        Console.WriteLine($"Name: {block.Name}");
        Console.WriteLine("");

        Console.WriteLine($"=== String table ({Size(block.StringTable.Size)}) ===");
        DisassembleStringTable(block.StringTable);
        Console.WriteLine("");

        Console.WriteLine("=== Functions ===");
        for (int i = 0; i < block.Functions.Count; i++) {
            DisassembleFunction(block.Functions[i], i);
        }
    }

    public void DisassembleStringTable (StringTable strTable) {
        _stringTable = strTable;

        for (int i = 0; i < strTable.Count; i++) {
            int offset = strTable.Offsets[i];

            Console.WriteLine(
                $"{i,4}   0x{offset:X4}    \"{strTable[i]}\""
            );
        }

        Console.WriteLine("");
    }

    public void DisassembleFunction (BinaryFunction func, int funcIndex) {
        Console.WriteLine($"== Function #0x{funcIndex,4:X4} ==");
        Console.WriteLine($"Name: {func.Name}");
        Console.WriteLine($"Name index: #0x{func.NameIndex,4:X4}");
        Console.WriteLine($"MaxLocals: {func.MaxLocals}");
        Console.WriteLine($"Parameters ({func.Arity}): Not implemented yet.");
        Console.WriteLine("");
        Console.WriteLine("= CHUNK =");
        DisassembleChunk(func.Chunk);
        Console.WriteLine("");
    }

    public void DisassembleChunk (Chunk chunk) {
        _chunk = chunk;

        int index = 0;
        while (index < chunk.Code.Count) {
            index = DisassembleInstruction(index);
            ChunkString += "\n";
        }

        Console.WriteLine(ChunkString); // TODO - don't do it like this.
        ChunkString = string.Empty;
    }


    private int DisassembleInstruction (int index) {
        OpCode opCode = (OpCode)_chunk.Code[index];
        ChunkString += $"Line {_chunk.Lines[index],-5} | {HexByteStr(index)} ";

        switch (opCode) {
            case OpCode.NOOP:
                return SimpleInstruction(nameof(OpCode.NOOP), index);
            case OpCode.NATIVE:
                return U64Instruction("*NATIVE", index);
            case OpCode.CONST:
                return ConstantInstruction(nameof(OpCode.CONST), index);
            case OpCode.CONST_L:
                return ConstantLongInstruction(nameof(OpCode.CONST_L), index);
            case OpCode.CONST_LL:
                return ConstantLongLongInstruction(nameof(OpCode.CONST_LL), index);
            case OpCode.CONST_0:
                return SimpleInstruction(nameof(OpCode.CONST_0), index);
            case OpCode.I_CONST_1:
                return SimpleInstruction(nameof(OpCode.I_CONST_1), index);
            case OpCode.I_CONST_2:
                return SimpleInstruction(nameof(OpCode.I_CONST_2), index);
            case OpCode.F_CONST_1:
                return SimpleInstruction(nameof(OpCode.F_CONST_1), index);
            case OpCode.F_CONST_2:
                return SimpleInstruction(nameof(OpCode.F_CONST_2), index);
            case OpCode.STR_CONST:
                return StringConstantInstruction(nameof(OpCode.STR_CONST), index);
            case OpCode.STR_CONST_L:
                return StringConstantLongInstruction(nameof(OpCode.STR_CONST_L), index);

            case OpCode.RET:
                return SimpleInstruction(nameof(OpCode.RET), index);

            case OpCode.F_NEG:
                return SimpleInstruction(nameof(OpCode.F_NEG), index);
            case OpCode.F_ADD:
                return SimpleInstruction(nameof(OpCode.F_ADD), index);
            case OpCode.F_SUB:
                return SimpleInstruction(nameof(OpCode.F_SUB), index);
            case OpCode.F_MUL:
                return SimpleInstruction(nameof(OpCode.F_MUL), index);
            case OpCode.F_DIV:
                return SimpleInstruction(nameof(OpCode.F_DIV), index);
            case OpCode.F_GT:
                return SimpleInstruction(nameof(OpCode.F_GT), index);
            case OpCode.F_GE:
                return SimpleInstruction(nameof(OpCode.F_GE), index);
            case OpCode.F_LT:
                return SimpleInstruction(nameof(OpCode.F_LT), index);
            case OpCode.F_LE:
                return SimpleInstruction(nameof(OpCode.F_LE), index);

            case OpCode.I_NEG:
                return SimpleInstruction(nameof(OpCode.I_NEG), index);
            case OpCode.I_ADD:
                return SimpleInstruction(nameof(OpCode.I_ADD), index);
            case OpCode.I_ADD_CHECKED:
                return SimpleInstruction(nameof(OpCode.I_ADD_CHECKED), index);
            case OpCode.I_SUB:
                return SimpleInstruction(nameof(OpCode.I_SUB), index);
            case OpCode.I_SUB_CHECKED:
                return SimpleInstruction(nameof(OpCode.I_SUB_CHECKED), index);
            case OpCode.I_MUL:
                return SimpleInstruction(nameof(OpCode.I_MUL), index);
            case OpCode.I_MUL_CHECKED:
                return SimpleInstruction(nameof(OpCode.I_MUL_CHECKED), index);
            case OpCode.I_DIV:
                return SimpleInstruction(nameof(OpCode.I_DIV), index);
            case OpCode.I_DIV_CHECKED:
                return SimpleInstruction(nameof(OpCode.I_DIV_CHECKED), index);
            case OpCode.I_GT:
                return SimpleInstruction(nameof(OpCode.I_GT), index);
            case OpCode.I_GE:
                return SimpleInstruction(nameof(OpCode.I_GE), index);
            case OpCode.I_LT:
                return SimpleInstruction(nameof(OpCode.I_LT), index);
            case OpCode.I_LE:
                return SimpleInstruction(nameof(OpCode.I_LE), index);

            case OpCode.EQ:
                return SimpleInstruction(nameof(OpCode.EQ), index);
            case OpCode.NEQ:
                return SimpleInstruction(nameof(OpCode.NEQ), index);

            case OpCode.STORE_0:
                return SimpleInstruction(nameof(OpCode.STORE_0), index);
            case OpCode.STORE_1:
                return SimpleInstruction(nameof(OpCode.STORE_1), index);
            case OpCode.STORE_2:
                return SimpleInstruction(nameof(OpCode.STORE_2), index);
            case OpCode.STORE_3:
                return SimpleInstruction(nameof(OpCode.STORE_3), index);
            case OpCode.STORE_4:
                return SimpleInstruction(nameof(OpCode.STORE_4), index);
            case OpCode.STORE:
                return ByteInstruction(nameof(OpCode.STORE), index);
            case OpCode.STORE_L:
                return U16Instruction(nameof(OpCode.STORE_L), index);

            case OpCode.LOAD_0:
                return SimpleInstruction(nameof(OpCode.LOAD_0), index);
            case OpCode.LOAD_1:
                return SimpleInstruction(nameof(OpCode.LOAD_1), index);
            case OpCode.LOAD_2:
                return SimpleInstruction(nameof(OpCode.LOAD_2), index);
            case OpCode.LOAD_3:
                return SimpleInstruction(nameof(OpCode.LOAD_3), index);
            case OpCode.LOAD_4:
                return SimpleInstruction(nameof(OpCode.LOAD_4), index);
            case OpCode.LOAD:
                return ByteInstruction(nameof(OpCode.LOAD), index);
            case OpCode.LOAD_L:
                return U16Instruction(nameof(OpCode.LOAD_L), index);

            case OpCode.JMP:
                return JumpInstruction(nameof(OpCode.JMP), index);
            case OpCode.JMP_L:
                return JumpLongInstruction(nameof(OpCode.JMP_L), index);
            case OpCode.JTRUE:
                return JumpInstruction(nameof(OpCode.JTRUE), index);
            case OpCode.JTRUE_L:
                return JumpLongInstruction(nameof(OpCode.JTRUE_L), index);
            case OpCode.JTRUE_K:
                return JumpInstruction(nameof(OpCode.JTRUE_K), index);
            case OpCode.JTRUE_K_L:
                return JumpLongInstruction(nameof(OpCode.JTRUE_K_L), index);
            case OpCode.JFALSE:
                return JumpInstruction(nameof(OpCode.JFALSE), index);
            case OpCode.JFALSE_L:
                return JumpLongInstruction(nameof(OpCode.JFALSE_L), index);
            case OpCode.JFALSE_K:
                return JumpInstruction(nameof(OpCode.JFALSE_K), index);
            case OpCode.JFALSE_K_L:
                return JumpLongInstruction(nameof(OpCode.JFALSE_K_L), index);

            case OpCode.CALL:
                return I32Instruction(nameof(OpCode.CALL), index);

            case OpCode.PRINT:
                return PrintInstruction(nameof(OpCode.PRINT), index);
        }

        return UnknownInstruction(index);
    }

    private int SimpleInstruction (string name, int index) {
        ChunkString += IdStr(name);
        return index + 1;
    }

    private int ByteInstruction (string name, int index) {
        var val = _chunk.Code[index + 1];

        ChunkString += IdStr(name) + " ";
        ChunkString += HexByteStr(val) + " ";

        return index + 2;
    }

    private int SByteInstruction (string name, int index) {
        sbyte val = unchecked((sbyte)_chunk.Code[index + 1]);

        ChunkString += IdStr(name) + " ";
        ChunkString += HexSByteStr(val) + " ";

        return index + 2;
    }

    private int U16Instruction (string name, int index) {
        var val = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8);

        ChunkString += IdStr(name) + " ";
        ChunkString += HexIntegerStr(val) + " ";

        return index + 3;
    }

    private int I32Instruction (string name, int index) {
        var val = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24);

        ChunkString += IdStr(name) + " ";
        ChunkString += HexIntegerStr(val) + " ";

        return index + 5;
    }

    private int U64Instruction (string name, int index) {
        var val = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24)
            | (_chunk.Code[index + 5] << 32)
            | (_chunk.Code[index + 6] << 40)
            | (_chunk.Code[index + 7] << 48)
            | (_chunk.Code[index + 8] << 56);

        ChunkString += IdStr(name) + " ";
        ChunkString += HexIntegerStr(val) + " ";

        return index + 9;
    }

    private int ConstantInstruction (string name, int index) {
        var constant = _chunk.Code[index + 1];

        ChunkString += IdStr(name) + " ";
        ChunkString += HexByteStr(constant) + " ; " + constant;

        return index + 2;
    }

    private int ConstantLongInstruction (string name, int index) {
        var constant = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24);

        ChunkString += IdStr(name) + " ";
        ChunkString += HexIntegerStr(constant) + " ; " + constant;

        return index + 5;
    }

    private int ConstantLongLongInstruction (string name, int index) {
        var constant = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24)
            | (_chunk.Code[index + 5] << 32)
            | (_chunk.Code[index + 6] << 40)
            | (_chunk.Code[index + 7] << 48)
            | (_chunk.Code[index + 8] << 56);

        ChunkString += IdStr(name) + " ";
        ChunkString += HexIntegerStr(constant) + " ; " + constant;

        return index + 9;
    }

    private int StringConstantInstruction (string name, int index) {
        var strIndex = _chunk.Code[index + 1];

        ChunkString += IdStr(name) + " ";
        ChunkString += HexByteStr(strIndex) + " ; " + _stringTable[strIndex];

        return index + 2;
    }

    private int StringConstantLongInstruction (string name, int index) {
        var strIndex = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24);

        ChunkString += IdStr(name) + " ";
        ChunkString += HexIntegerStr(strIndex) + " ; " + _stringTable[strIndex];

        return index + 5;
    }

    private int JumpInstruction (string name, int index) {
        sbyte val = unchecked((sbyte)_chunk.Code[index + 1]);

        ChunkString += IdStr(name) + " ";
        ChunkString += HexSByteStr(val) + " ; to " + HexSByteStr(index + val + 2);

        return index + 2;
    }

    private int JumpLongInstruction (string name, int index) {
        var val = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24);

        ChunkString += IdStr(name) + " ";
        ChunkString += HexIntegerStr(val) + " ; to " + HexSByteStr(index + val + 5);

        return index + 5;
    }

    private int PrintInstruction (string name, int index) {
        var constType = _chunk.Code[index + 1];

        ChunkString += IdStr(name) + " ";
        ChunkString += HexByteStr(constType) + " ; " + (ConstantType)constType;

        return index + 2;
    }

    private int UnknownInstruction (int index) {
        ChunkString += $"0x{index:X4} <Unknown>";
        return index + 1;
    }

    private static string HexByteStr (int index) => $"0x{index:X4}";
    private static string HexSByteStr (int index) => $"0x{index:X4}";
    private static string HexIntegerStr (long index) => $"0x{index:X8}";
    private static string JFloatStr (double val) => $"{val}";
    private static string IdStr (string id) => id.PadRight(16, ' ');

    private static string Size (int size) {
        if (size < 1_024) {
            return $"{size} bytes";
        }

        float fsize = size / 1_024f;

        for (int i = 0; i < _sizeUnits.Length - 1; i++) {
            if (fsize < 1_024) {
                return $"{fsize:F3} {_sizeUnits[i]}";
            }
            fsize /= 1_024f;
        }

        return $"{fsize:F3} {_sizeUnits[^1]}";
    }
}
