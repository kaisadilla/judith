#include <sstream>
#include <iomanip>

#include "debug/chunk.hpp"
#include "Chunk.hpp"
#include "jal/opcodes.hpp"

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

static size_t constantInstruction (std::ostringstream& str, Chunk& chunk, const char* name, size_t index) {
    if (index + 1 >= chunk.size) {
        std::cerr << "Constant instruction at index " << index << " overflows the code array.";
        return index + 1;
    }

    byte constIndex = chunk.code[index + 1];
    Value constant = chunk.constants[constIndex];

    str << idStr(name) << " " << hexByteStr(constIndex) << " ";

    if (constant.type == ValueType::FLOAT64) {
        str << "; " << floatStr(constant.as.floatNum);
    }
    else {
        str << "<unknown value type>";
    }

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
    Value constant = chunk.constants[constIndex];

    str << idStr(name) << " " << hexIntegerStr(constIndex) << " ";

    if (constant.type == ValueType::FLOAT64) {
        str << "; " << floatStr(constant.as.floatNum);
    }
    else {
        str << "<unknown value type>";
    }

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
    case OpCode::CONSTANT:
        return constantInstruction(str, chunk, "CONSTANT", index);
    case OpCode::CONSTANT_LONG:
        return constantLongInstruction(str, chunk, "CONSTANT_LONG", index);
    case OpCode::RETURN:
        return simpleInstruction(str, chunk, "RETURN", index);
    case OpCode::NEGATE:
        return simpleInstruction(str, chunk, "NEGATE", index);
    case OpCode::ADD:
        return simpleInstruction(str, chunk, "ADD", index);
    case OpCode::SUBTRACT:
        return simpleInstruction(str, chunk, "SUBTRACT", index);
    case OpCode::MULTIPLY:
        return simpleInstruction(str, chunk, "MULTIPLY", index);
    case OpCode::DIVIDE:
        return simpleInstruction(str, chunk, "DIVIDE", index);
    case OpCode::CHECKED_ADD:
        return simpleInstruction(str, chunk, "CHECKED_ADD", index);
    case OpCode::CHECKED_SUBTRACT:
        return simpleInstruction(str, chunk, "CHECKED_SUBTRACT", index);
    case OpCode::CHECKED_MULTIPLY:
        return simpleInstruction(str, chunk, "CHECKED_MULTIPLY", index);
    case OpCode::CHECKED_DIVIDE:
        return simpleInstruction(str, chunk, "CHECKED_DIVIDE", index);
    case OpCode::PRINT:
        return simpleInstruction(str, chunk, "PRINT", index);
    default:
        break;
    }
}