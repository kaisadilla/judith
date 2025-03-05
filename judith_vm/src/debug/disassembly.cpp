#include <sstream>
#include <iomanip>
#include <format>

#include "debug/disassembly.hpp"
#include "executable/ConstantType.hpp"
#include "executable/Chunk.hpp"
#include "executable/Block.hpp"
#include "jal/opcodes.hpp"
#include "Value.hpp"

void disassembleChunk (std::ostringstream& str, Chunk& chunk);
size_t disassembleInstruction (std::ostringstream& str, Chunk& chunk, size_t index);

#pragma region Formatters
static std::string hexIntStr (i64 val) {
    std::ostringstream str;
    str << "0x" << std::setw(4) << std::right << std::setfill('0') << std::hex << (int)val;
    return str.str();
}

static std::string hexIntLongStr (i64 val) {
    std::ostringstream str;
    str << "0x" << std::setw(8) << std::setfill('0') << std::hex << val;
    return str.str();
}

static std::string floatStr (f64 val) {
    std::ostringstream str;
    str << val;
    return str.str();
}

static std::string idStr (const char* id) {
    std::ostringstream str;
    str << std::setw(16) << std::left << std::setfill(' ') << id;
    return str.str();
}

static std::string stringConstant (Chunk& chunk, size_t constIndex) {
    byte* strval = (byte*)chunk.strings[constIndex];
    ui64 strlen = *(ui64*)strval;
    byte* strStart = strval + sizeof(ui64);
    return '"' + std::string((char*)strStart, strlen) + '"';
}

static std::string constTypeStr (byte type) {
    switch (type) {
        case ConstantType::ERROR:
            return "<error-type>";
        case ConstantType::INT_64:
            return "INT_64";
        case ConstantType::FLOAT_64:
            return "FLOAT_64";
        case ConstantType::UNSIGNED_INT_64:
            return "UNSIGNED_INT_64";
        case ConstantType::STRING_ASCII:
            return "STRING_ASCII";
        case ConstantType::BOOL:
            return "BOOL";
        default:
            return "<unknown-type>";
    }

}

#pragma endregion

#pragma region Disassembly functions

static size_t unknownInstruction (std::ostringstream& str, size_t index) {
    str << hexIntStr(index);
    str << " <Unknown>";
    return index + 1;
}

static size_t simpleInstruction (std::ostringstream& str, Chunk& chunk, const char* name, size_t index) {
    str << idStr(name);
    return index + 1;
}

static size_t byteInstruction (std::ostringstream& str, Chunk& chunk, const char* name, size_t index) {
    if (index + 1 >= chunk.size) {
        std::cerr << "Byte instruction at index " << index << " overflows the code array.";
        return index + 2;
    }

    byte val = chunk.code[index + 1];

    str << idStr(name) << " " << hexIntStr(val) << " ";

    return index + 2;
}

static size_t u16Instruction (std::ostringstream& str, Chunk& chunk, const char* name, size_t index) {
    if (index + 2 >= chunk.size) {
        std::cerr << "U16 instruction at index " << index << " overflows the code array.";
        return index + 3;
    }

    i32 val = chunk.code[index + 1]
        + (chunk.code[index + 2] << 8);

    str << idStr(name) << " " << hexIntLongStr(val) << " ";

    return index + 3;
}

static size_t u32Instruction (std::ostringstream& str, Chunk& chunk, const char* name, size_t index) {
    if (index + 2 >= chunk.size) {
        std::cerr << "U32 instruction at index " << index << " overflows the code array.";
        return index + 5;
    }

    ui32 val = chunk.code[index + 1]
        + (chunk.code[index + 2] << 8)
        + (chunk.code[index + 3] << 16)
        + (chunk.code[index + 4] << 24);

    str << idStr(name) << " " << hexIntLongStr(val) << " ";

    return index + 5;
}

static size_t constantInstruction (
    std::ostringstream& str, Chunk& chunk, const char* name, size_t index
) {
    if (index + 1 >= chunk.size) {
        std::cerr << "Constant instruction at index " << index << " overflows the code array.";
        return index + 2;
    }

    byte val = chunk.code[index + 1];

    str << idStr(name) << " " << hexIntStr(val) << " ; " << (int)val;

    return index + 2;
}

static size_t constantLongInstruction (
    std::ostringstream& str, Chunk& chunk, const char* name, size_t index
) {
    if (index + 4 >= chunk.size) {
        std::cerr << "Constant long instruction at index " << index << " overflows the code array.";
        return index + 5;
    }

    i32 val = chunk.code[index + 1]
        + (chunk.code[index + 2] << 8)
        + (chunk.code[index + 3] << 16)
        + (chunk.code[index + 4] << 24);

    str << idStr(name) << " " << hexIntLongStr(val) << " ; " << val;

    return index + 5;
}

static size_t constantLongLongInstruction (
    std::ostringstream& str, Chunk& chunk, const char* name, size_t index
) {
    if (index + 4 >= chunk.size) {
        std::cerr << "Constant long instruction at index " << index << " overflows the code array.";
        return index + 9;
    }

    i64 val = *(i64*)(&chunk.code[index]);

    str << idStr(name) << " " << hexIntLongStr(val) << " ; " << val;

    return index + 9;
}

static size_t stringConstantInstruction (
    std::ostringstream& str, Chunk& chunk, const char* name, size_t index
) {
    if (index + 1 >= chunk.size) {
        std::cerr << "Constant instruction at index " << index << " overflows the code array.";
        return index + 2;
    }

    byte strIndex = chunk.code[index + 1];

    str << idStr(name) << " " << hexIntStr(strIndex) << " ; " << stringConstant(chunk, strIndex);

    return index + 2;
}

static size_t stringConstantLongInstruction (
    std::ostringstream& str, Chunk& chunk, const char* name, size_t index
) {
    if (index + 4 >= chunk.size) {
        std::cerr << "Constant long instruction at index " << index << " overflows the code array.";
        return index + 5;
    }

    i32 strIndex = chunk.code[index + 1]
        + (chunk.code[index + 2] << 8)
        + (chunk.code[index + 3] << 16)
        + (chunk.code[index + 4] << 24);

    str << idStr(name) << " " << hexIntLongStr(strIndex) << " ; " << stringConstant(chunk, strIndex);

    return index + 5;
}

static size_t jumpInstruction (std::ostringstream& str, Chunk& chunk, const char* name, size_t index) {
    if (index + 1 >= chunk.size) {
        std::cerr << "Jump instruction at index " << index << " overflows the code array.";
        return index + 2;
    }

    sbyte val = (sbyte)chunk.code[index + 1];

    str << idStr(name) << " " << hexIntStr(val) << " ; " << hexIntStr(index + val + 2);

    return index + 2;
}

static size_t jumpLongInstruction (std::ostringstream& str, Chunk& chunk, const char* name, size_t index) {
    if (index + 2 >= chunk.size) {
        std::cerr << "JumpLong instruction at index " << index << " overflows the code array.";
        return index + 5;
    }

    i32 val = chunk.code[index + 1]
        + (chunk.code[index + 2] << 8)
        + (chunk.code[index + 3] << 16)
        + (chunk.code[index + 4] << 24);

    str << idStr(name) << " " << hexIntStr(val) << " ; " << hexIntStr(index + val + 4);

    return index + 5;
}

static size_t printInstruction (std::ostringstream& str, Chunk& chunk, const char* name, size_t index) {
    if (index + 1 >= chunk.size) {
        std::cerr << "Print instruction at index " << index << " overflows the code array.";
        return index + 2;
    }

    byte constType = chunk.code[index + 1];

    str << idStr(name) << " " << hexIntStr(constType) << " ; " << constTypeStr(constType);

    return index + 2;
}
#pragma endregion

std::string disassembleBlock(Block& block) {
    std::ostringstream str;

    for (size_t i = 0; i < block.functionCount; i++) {
        str << "== FUNCTION #" << i << " ==\n";
        disassembleChunk(str, block.functions[i].chunk);
    }

    return str.str();
}

static void disassembleChunk (std::ostringstream& str, Chunk& chunk) {
    size_t index = 0;
    while (index < chunk.size) {
        index = disassembleInstruction(str, chunk, index);
        str << "\n";
    }
    str << "\n";
}

static size_t disassembleInstruction (std::ostringstream& str, Chunk& chunk, size_t index) {
    int opCode = chunk.code[index];

    if (chunk.containsLines) {
        str << "Line " << std::setw(5) << std::left << chunk.lines[index] << std::setw(0) << " | ";
    }
    str << hexIntStr(index) << " ";

    switch (opCode) {
    case OpCode::NOOP:
        return simpleInstruction(str, chunk, "NOOP", index);

    case OpCode::CONST:
        return constantInstruction(str, chunk, "CONST", index);
    case OpCode::CONST_L:
        return constantLongInstruction(str, chunk, "CONST_L", index);
    case OpCode::CONST_L_L:
        return constantLongLongInstruction(str, chunk, "CONST_LL", index);
    case OpCode::CONST_0:
        return simpleInstruction(str, chunk, "CONST_0", index);
    case OpCode::F_CONST_1:
        return simpleInstruction(str, chunk, "F_CONST_1", index);
    case OpCode::F_CONST_2:
        return simpleInstruction(str, chunk, "F_CONST_2", index);
    case OpCode::I_CONST_1:
        return simpleInstruction(str, chunk, "I_CONST_1", index);
    case OpCode::I_CONST_2:
        return simpleInstruction(str, chunk, "I_CONST_2", index);
    case OpCode::STR_CONST:
        return stringConstantInstruction(str, chunk, "STR_CONST", index);
    case OpCode::STR_CONST_L:
        return stringConstantLongInstruction(str, chunk, "STR_CONST_L", index);

    case OpCode::RET:
        return simpleInstruction(str, chunk, "RET", index);
    case OpCode::F_NEG:
        return simpleInstruction(str, chunk, "F_NEG", index);
    case OpCode::F_ADD:
        return simpleInstruction(str, chunk, "F_ADD", index);
    case OpCode::F_SUB:
        return simpleInstruction(str, chunk, "F_SUB", index);
    case OpCode::F_MUL:
        return simpleInstruction(str, chunk, "F_MUL", index);
    case OpCode::F_DIV:
        return simpleInstruction(str, chunk, "F_DIV", index);
    case OpCode::F_GT:
        return simpleInstruction(str, chunk, "F_GT", index);
    case OpCode::F_GE:
        return simpleInstruction(str, chunk, "F_GE", index);
    case OpCode::F_LT:
        return simpleInstruction(str, chunk, "F_LT", index);
    case OpCode::F_LE:
        return simpleInstruction(str, chunk, "F_LE", index);

    case OpCode::I_NEG:
        return simpleInstruction(str, chunk, "I_NEG", index);
    case OpCode::I_ADD:
        return simpleInstruction(str, chunk, "I_ADD", index);
    case OpCode::I_ADD_CHECKED:
        return simpleInstruction(str, chunk, "I_ADD_CHECKED", index);
    case OpCode::I_SUB:
        return simpleInstruction(str, chunk, "I_SUB", index);
    case OpCode::I_SUB_CHECKED:
        return simpleInstruction(str, chunk, "I_SUB_CHECKED", index);
    case OpCode::I_MUL:
        return simpleInstruction(str, chunk, "I_MUL", index);
    case OpCode::I_MUL_CHECKED:
        return simpleInstruction(str, chunk, "I_MUL_CHECKED", index);
    case OpCode::I_DIV:
        return simpleInstruction(str, chunk, "I_DIV", index);
    case OpCode::I_DIV_CHECKED:
        return simpleInstruction(str, chunk, "I_DIV_CHECKED", index);
    case OpCode::I_GT:
        return simpleInstruction(str, chunk, "I_GT", index);
    case OpCode::I_GE:
        return simpleInstruction(str, chunk, "I_GE", index);
    case OpCode::I_LT:
        return simpleInstruction(str, chunk, "I_LT", index);
    case OpCode::I_LE:
        return simpleInstruction(str, chunk, "I_LE", index);

    case OpCode::EQ:
        return simpleInstruction(str, chunk, "EQ", index);
    case OpCode::NEQ:
        return simpleInstruction(str, chunk, "NEQ", index);

    case OpCode::STORE_0:
        return simpleInstruction(str, chunk, "STORE_0", index);
    case OpCode::STORE_1:
        return simpleInstruction(str, chunk, "STORE_1", index);
    case OpCode::STORE_2:
        return simpleInstruction(str, chunk, "STORE_2", index);
    case OpCode::STORE_3:
        return simpleInstruction(str, chunk, "STORE_3", index);
    case OpCode::STORE_4:
        return simpleInstruction(str, chunk, "STORE_4", index);
    case OpCode::STORE:
        return byteInstruction(str, chunk, "STORE", index);
    case OpCode::STORE_L:
        return u16Instruction(str, chunk, "STORE_L", index);

    case OpCode::LOAD_0:
        return simpleInstruction(str, chunk, "LOAD_0", index);
    case OpCode::LOAD_1:
        return simpleInstruction(str, chunk, "LOAD_1", index);
    case OpCode::LOAD_2:
        return simpleInstruction(str, chunk, "LOAD_2", index);
    case OpCode::LOAD_3:
        return simpleInstruction(str, chunk, "LOAD_3", index);
    case OpCode::LOAD_4:
        return simpleInstruction(str, chunk, "LOAD_4", index);
    case OpCode::LOAD:
        return byteInstruction(str, chunk, "LOAD", index);

    case OpCode::LOAD_L:
        return u16Instruction(str, chunk, "LOAD_L", index);

    case OpCode::JMP:
        return jumpInstruction(str, chunk, "JMP", index);
    case OpCode::JMP_L:
        return jumpLongInstruction(str, chunk, "JMP_L", index);
    case OpCode::JTRUE:
        return jumpInstruction(str, chunk, "JTRUE", index);
    case OpCode::JTRUE_L:
        return jumpLongInstruction(str, chunk, "JTRUE_L", index);
    case OpCode::JTRUE_K:
        return jumpInstruction(str, chunk, "JTRUE_K", index);
    case OpCode::JTRUE_K_L:
        return jumpLongInstruction(str, chunk, "JTRUE_K_L", index);
    case OpCode::JFALSE:
        return jumpInstruction(str, chunk, "JFALSE", index);
    case OpCode::JFALSE_L:
        return jumpLongInstruction(str, chunk, "JFALSE_L", index);
    case OpCode::JFALSE_K:
        return jumpInstruction(str, chunk, "JFALSE_K", index);
    case OpCode::JFALSE_K_L:
        return jumpLongInstruction(str, chunk, "JFALSE_K_L", index);

    case OpCode::CALL:
        return u32Instruction(str, chunk, "CALL", index);

    case OpCode::PRINT:
        return printInstruction(str, chunk, "PRINT", index);
    default:
        break;
    }
}