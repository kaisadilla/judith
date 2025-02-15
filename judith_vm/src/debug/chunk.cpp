#include <sstream>
#include <iomanip>
#include <format>

#include "debug/chunk.hpp"
#include "Chunk.hpp"
#include "jal/opcodes.hpp"
#include "Value.hpp"

size_t disassembleInstruction (std::ostringstream& str, Chunk& chunk, size_t index);

#pragma region Formatters
static std::string hexByteStr (byte val) {
    std::ostringstream str;
    str << "0x" << std::setw(4) << std::right << std::setfill('0') << std::hex << (int)val;
    return str.str();
}

static std::string hexIntegerStr (i64 val) {
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

static std::string constant (Chunk& chunk, size_t constIndex) {
    byte type = chunk.constantTypes[constIndex];

    switch (type) {
        case ConstantType::ERROR:
            return "<error-type>";
        case ConstantType::INT_64: {
            i64 ival = *(i64*)chunk.constants[constIndex];
            return std::format("{}", ival);
        }
        case ConstantType::FLOAT_64: {
            f64 fval = *(f64*)chunk.constants[constIndex];
            return std::format("{}", fval);
        }
        case ConstantType::UNSIGNED_INT_64: {
            ui64 uval = *(ui64*)chunk.constants[constIndex];
            return std::format("{}", uval);
        }
        case ConstantType::STRING_ASCII: {
            byte* strvalue = (byte*)chunk.constants[constIndex];
            ui64 strlen = *(ui64*)strvalue;
            byte* strStart = strvalue + sizeof(ui64);
            return '"' + std::string((char*)strStart, strlen) + '"';
        }
    }

}

#pragma endregion

#pragma region Disassembly functions

static size_t unknownInstruction (std::ostringstream& str, size_t index) {
    str << hexByteStr(index);
    str << " <Unknown>";
    return index + 1;
}

static size_t simpleInstruction (std::ostringstream& str, Chunk& chunk, const char* name, size_t index) {
    str << idStr(name);
    return index + 1;
}

static size_t byteInstruction (std::ostringstream& str, Chunk& chunk, const char* name, size_t index) {
    if (index + 1 >= chunk.size) {
        std::cerr << "Constant instruction at index " << index << " overflows the code array.";
        return index + 2;
    }

    byte val = chunk.code[index + 1];

    str << idStr(name) << " " << hexByteStr(val) << " ";

    return index + 2;
}

static size_t u16Instruction (std::ostringstream& str, Chunk& chunk, const char* name, size_t index) {
    if (index + 2 >= chunk.size) {
        std::cerr << "Constant long instruction at index " << index << " overflows the code array.";
        return index + 3;
    }

    i32 val = chunk.code[index + 1]
        + (chunk.code[index + 2] << 8);

    str << idStr(name) << " " << hexIntegerStr(val) << " ";

    return index + 3;
}

static size_t constantInstruction (std::ostringstream& str, Chunk& chunk, const char* name, size_t index) {
    if (index + 1 >= chunk.size) {
        std::cerr << "Constant instruction at index " << index << " overflows the code array.";
        return index + 2;
    }

    byte constIndex = chunk.code[index + 1];

    str << idStr(name) << " " << hexByteStr(constIndex) << "; " << constant(chunk, constIndex);

    return index + 2;
}

static size_t constantLongInstruction (std::ostringstream& str, Chunk& chunk, const char* name, size_t index) {
    if (index + 4 >= chunk.size) {
        std::cerr << "Constant long instruction at index " << index << " overflows the code array.";
        return index + 5;
    }

    i32 constIndex = chunk.code[index + 1]
        + (chunk.code[index + 2] << 8)
        + (chunk.code[index + 2] << 16)
        + (chunk.code[index + 2] << 24);

    str << idStr(name) << " " << hexIntegerStr(constIndex) << "; " << constant(chunk, constIndex);

    return index + 5;
}
#pragma endregion

std::string disassembleChunk(Chunk& chunk) {
    std::ostringstream str;

    size_t index = 0;
    while (index < chunk.size) {
        index = disassembleInstruction(str, chunk, index);
        str << "\n";
    }

    return str.str();
}

static size_t disassembleInstruction (std::ostringstream& str, Chunk& chunk, size_t index) {
    int opCode = chunk.code[index];

    if (chunk.containsLines) {
        str << "Line " << chunk.lines[index] << " | ";
    }
    str << hexByteStr(index) << " ";

    switch (opCode) {
    case OpCode::NOOP:
        return simpleInstruction(str, chunk, "NOOP", index);

    case OpCode::CONST:
        return constantInstruction(str, chunk, "CONST", index);
    case OpCode::CONST_LONG:
        return constantLongInstruction(str, chunk, "CONST_L", index);
    case OpCode::CONST_0:
        return simpleInstruction(str, chunk, "CONST_0", index);
    case OpCode::I_CONST_1:
        return simpleInstruction(str, chunk, "I_CONST_1", index);
    case OpCode::I_CONST_2:
        return simpleInstruction(str, chunk, "I_CONST_2", index);

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

    case OpCode::PRINT:
        return simpleInstruction(str, chunk, "PRINT", index);
    default:
        break;
    }
}