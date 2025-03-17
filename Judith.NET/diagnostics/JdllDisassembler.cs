using Judith.NET.codegen.jasm;
using Judith.NET.ir;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.diagnostics;

public class JdllDisassembler {
    private static string[] _sizeUnits = ["KiB", "MiB", "GiB", "TiB"];

    private JasmAssembly _assembly;
    private StringBuilder _buffer = new();

    private JasmBlock _block = null!;
    private Chunk _chunk = null!;

    public string? Disassembly { get; private set; }

    public JdllDisassembler (JasmAssembly dll) {
        _assembly = dll;
    }

    public void Disassemble () {
        _buffer.Clear();

        _buffer.Append("=== HEADER ===\n");
        _buffer.Append($"judith_version: {_assembly.JudithVersion}\n");
        _buffer.Append($"version: {_assembly.Version}\n");
        _buffer.Append('\n');

        _buffer.Append($"=== NAME TABLE ({Size(_assembly.NameTable.Size)}) ===\n");
        DisassembleStringTable(_assembly.NameTable);
        _buffer.Append('\n');

        _buffer.Append("=== TYPE REFERENCE TABLE ===\n");
        DisassembleRefTable(_assembly.TypeRefTable);
        _buffer.Append('\n');

        _buffer.Append("=== FUNCTION REFERENCE TABLE ===\n");
        DisassembleRefTable(_assembly.FunctionRefTable);
        _buffer.Append('\n');

        _buffer.Append($"=== BLOCKS ({_assembly.Blocks.Count}) ===\n");

        for (int i = 0; i < _assembly.Blocks.Count; i++) {
            DisassembleBlock(_assembly.Blocks[i], i);
        }

        Disassembly = _buffer.ToString();
    }

    public void DisassembleRefTable (JasmRefTable table) {
        for (int i = 0; i < table.Size; i++) {
            _buffer.Append($"0x{i,4:X4} ({table[i].RefType}):\n");

            switch (table[i]) {
                case JasmInternalRef internalRef: {
                    _buffer.Append($" - block: {internalRef.Block}\n");
                    _buffer.Append($" - index: {internalRef.Index}\n");
                    break;
                }
                case JasmNativeRef nativeRef: {
                    _buffer.Append($" - index: {nativeRef.Index}\n");
                    break;
                }
                case JasmExternalRef externalRef: {
                    var blockName = _assembly.NameTable[externalRef.BlockName];
                    var itemName = _assembly.NameTable[externalRef.ItemName];

                    _buffer.Append($" - block: {externalRef.BlockName}\n");
                    _buffer.Append($" - item: {externalRef.ItemName}\n");
                    break;
                }
            }
        }
    }

    public void DisassembleBlock (JasmBlock block, int blockIndex) {
        _block = block;

        string blockName = _assembly.NameTable[block.NameIndex];

        _buffer.Append($"== Block #0x{blockIndex,4:X4} ==\n");
        _buffer.Append($"Name: {block.NameIndex} ; \"{blockName}\"\n");
        _buffer.Append('\n');

        _buffer.Append($"== String table ({Size(block.StringTable.Size)}) ==\n");
        DisassembleStringTable(block.StringTable);
        _buffer.Append('\n');

        _buffer.Append($"== Types ==\n");
        // TODO.
        _buffer.Append('\n');

        _buffer.Append($"== Functions ==\n");
        for (int i = 0; i < block.FunctionTable.Count; i++) {
            DisassembleFunction(block, i);
        }
        _buffer.Append('\n');

    }

    public void DisassembleStringTable (StringTable strTable) {
        for (int i = 0; i < strTable.Count; i++) {
            int offset = strTable.Offsets[i];

            _buffer.Append($"{i,4}   0x{offset:X4}    \"{strTable[i]}\"\n");
        }
    }

    public void DisassembleFunction (JasmBlock block, int funcIndex) {
        var func = block.FunctionTable[funcIndex];
        string funcName = block.StringTable[func.NameIndex];

        _buffer.Append($"= Function #0x{funcIndex,4:X4} =\n");
        _buffer.Append($"name: {func.NameIndex} ; \"{funcName}\"\n");
        _buffer.Append($"max_locals: {func.MaxLocals}\n");
        // TODO: Max stack.

        _buffer.Append($"parameters ({func.Arity}):\n");
        for (int i = 0; i < func.Parameters.Count; i++) {
            DisassembleParameters(block, func, i);
        }
        _buffer.Append('\n');

        _buffer.Append("chunk:\n");
        DisassembleChunk(func.Chunk);
        _buffer.Append('\n');
    }

    public void DisassembleParameters (
        JasmBlock block, JasmFunction func, int paramIndex
    ) {
        var p = func.Parameters[paramIndex];
        string paramName = block.StringTable[p.NameIndex];

        _buffer.Append($"{paramIndex,4} - name: {p.NameIndex} ; \"{paramName}\"\n");

    }

    public void DisassembleChunk (Chunk chunk) {
        _chunk = chunk;

        int index = 0;
        while (index < chunk.Code.Count) {
            index = DisassembleInstruction(index);
            _buffer.Append('\n');
        }
    }

    private int DisassembleInstruction (int index) {
        OpCode opCode = (OpCode)_chunk.Code[index];
        _buffer.Append($"Line {-10,-5} | {HexByteStr(index)} ");
        Func<int, int> a;

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

            case OpCode.POP:
                return SimpleInstruction(nameof(OpCode.POP), index);

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
        _buffer.Append(IdStr(name));
        return index + 1;
    }

    private int ByteInstruction (string name, int index) {
        var val = _chunk.Code[index + 1];

        _buffer.Append(IdStr(name) + " ");
        _buffer.Append(HexByteStr(val) + " ");

        return index + 2;
    }

    private int SByteInstruction (string name, int index) {
        sbyte val = unchecked((sbyte)_chunk.Code[index + 1]);

        _buffer.Append(IdStr(name) + " ");
        _buffer.Append(HexSByteStr(val) + " ");

        return index + 2;
    }

    private int U16Instruction (string name, int index) {
        var val = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8);

        _buffer.Append(IdStr(name) + " ");
        _buffer.Append(HexIntegerStr(val) + " ");

        return index + 3;
    }

    private int I32Instruction (string name, int index) {
        var val = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24);

        _buffer.Append(IdStr(name) + " ");
        _buffer.Append(HexIntegerStr(val) + " ");

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

        _buffer.Append(IdStr(name) + " ");
        _buffer.Append(HexIntegerStr(val) + " ");

        return index + 9;
    }

    private int ConstantInstruction (string name, int index) {
        var constant = _chunk.Code[index + 1];

        _buffer.Append(IdStr(name) + " ");
        _buffer.Append(HexByteStr(constant) + " ; " + constant);

        return index + 2;
    }

    private int ConstantLongInstruction (string name, int index) {
        var constant = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24);

        _buffer.Append(IdStr(name) + " ");
        _buffer.Append(HexIntegerStr(constant) + " ; " + constant);

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

        _buffer.Append(IdStr(name) + " ");
        _buffer.Append(HexIntegerStr(constant) + " ; " + constant);

        return index + 9;
    }

    private int StringConstantInstruction (string name, int index) {
        var strIndex = _chunk.Code[index + 1];

        _buffer.Append(IdStr(name) + " ");
        _buffer.Append(HexByteStr(strIndex) + " ; " + _block.StringTable[strIndex]);

        return index + 2;
    }

    private int StringConstantLongInstruction (string name, int index) {
        var strIndex = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24);

        _buffer.Append(IdStr(name) + " ");
        _buffer.Append(HexIntegerStr(strIndex) + " ; " + _block.StringTable[strIndex]);

        return index + 5;
    }

    private int JumpInstruction (string name, int index) {
        sbyte val = unchecked((sbyte)_chunk.Code[index + 1]);

        _buffer.Append(IdStr(name) + " ");
        _buffer.Append(HexSByteStr(val) + " ; to " + HexSByteStr(index + val + 2));

        return index + 2;
    }

    private int JumpLongInstruction (string name, int index) {
        var val = _chunk.Code[index + 1]
            | (_chunk.Code[index + 2] << 8)
            | (_chunk.Code[index + 3] << 16)
            | (_chunk.Code[index + 4] << 24);

        _buffer.Append(IdStr(name) + " ");
        _buffer.Append(HexIntegerStr(val) + " ; to " + HexSByteStr(index + val + 5));

        return index + 5;
    }

    private int PrintInstruction (string name, int index) {
        var constType = _chunk.Code[index + 1];

        _buffer.Append(IdStr(name) + " ");
        _buffer.Append(HexByteStr(constType) + " ; " + (JasmConstantType)constType);

        return index + 2;
    }

    private int UnknownInstruction (int index) {
        _buffer.Append($"0x{index:X4} <Unknown>");
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
