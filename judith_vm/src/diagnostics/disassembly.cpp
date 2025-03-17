#include <sstream>
#include <iomanip>
#include <format>

#include "diagnostics/disassembly.hpp"
#include "runtime/ConstantType.hpp"
#include "jasm/opcodes.hpp"
#include "runtime/Value.hpp"
#include "data/AssemblyFile.hpp"
#include "data/StringTable.hpp"

static void disassembleStringTable (std::ostringstream& str, const StringTable& table);
static void disassembleRefTable (std::ostringstream& str, const ItemRefTable& table);
static std::string disassembleBlock (const AssemblyFile& file, size_t index);
static std::string disassembleFunction (const AssemblyFile& file, const BinaryBlock& block, size_t index);
static void disassembleParameter (std::ostringstream& str, const BinaryBlock& block, const BinaryFunction& func, size_t index);
static std::string disassembleChunk (const BinaryBlock& block, const std::vector<byte>& chunk);
static size_t disassembleInstruction (std::ostringstream& str, const BinaryBlock& block, const std::vector<byte>& chunk, size_t index);

#pragma region Formatters
static std::string decIntStr (i64 val) {
    std::ostringstream str;
    str << std::setw(4) << std::right << std::setfill('0') << std::dec << val;
    return str.str();
}

static std::string hexIntStr (i64 val) {
    std::ostringstream str;
    str << "0x" << std::setw(4) << std::right << std::setfill('0') << std::hex << val;
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

static std::string sizeStr (size_t size) {
    std::stringstream str;
    if (size < 1'024) {
        str << size << " bytes";
        return str.str();
    }

    f64 fsize = size / 1'024.f;

    if (fsize < 1'024) {
        str << size << " KiB";
        return str.str();
    }

    fsize /= 1'024.f;

    str << size << " MiB";
    return str.str();
}

static std::string idStr (const char* id) {
    std::ostringstream str;
    str << std::setw(16) << std::left << std::setfill(' ') << id;
    return str.str();
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
        case ConstantType::STRING_UTF8:
            return "STRING_UTF8";
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

static size_t simpleInstruction (std::ostringstream& str, const char* name, size_t index) {
    str << idStr(name);
    return index + 1;
}

static size_t byteInstruction (std::ostringstream& str, const std::vector<byte>& chunk, const char* name, size_t index) {
    if (index + 1 >= chunk.size()) {
        std::cerr << "Byte instruction at index " << index << " overflows the code array.";
        return index + 2;
    }

    byte val = chunk.at(index + 1);

    str << idStr(name) << " " << hexIntStr(val) << " ";

    return index + 2;
}

static size_t u16Instruction (std::ostringstream& str, const std::vector<byte>& chunk, const char* name, size_t index) {
    if (index + 2 >= chunk.size()) {
        std::cerr << "U16 instruction at index " << index << " overflows the code array.";
        return index + 3;
    }

    i32 val = chunk.at(index + 1)
        + (chunk.at(index + 2) << 8);

    str << idStr(name) << " " << hexIntLongStr(val) << " ";

    return index + 3;
}

static size_t u32Instruction (std::ostringstream& str, const std::vector<byte>& chunk, const char* name, size_t index) {
    if (index + 2 >= chunk.size()) {
        std::cerr << "U32 instruction at index " << index << " overflows the code array.";
        return index + 5;
    }

    ui32 val = chunk.at(index + 1)
        + (chunk.at(index + 2) << 8)
        + (chunk.at(index + 3) << 16)
        + (chunk.at(index + 4) << 24);

    str << idStr(name) << " " << hexIntLongStr(val) << " ";

    return index + 5;
}

static size_t constantInstruction (
    std::ostringstream& str, const std::vector<byte>& chunk, const char* name, size_t index
) {
    if (index + 1 >= chunk.size()) {
        std::cerr << "Constant instruction at index " << index << " overflows the code array.";
        return index + 2;
    }

    byte val = chunk.at(index + 1);

    str << idStr(name) << " " << hexIntStr(val) << " ; " << (int)val;

    return index + 2;
}

static size_t constantLongInstruction (
    std::ostringstream& str, const std::vector<byte>& chunk, const char* name, size_t index
) {
    if (index + 4 >= chunk.size()) {
        std::cerr << "Constant long instruction at index " << index << " overflows the code array.";
        return index + 5;
    }

    i32 val = *(i32*)(&chunk.at(index));

    str << idStr(name) << " " << hexIntLongStr(val) << " ; " << val;

    return index + 5;
}

static size_t constantLongLongInstruction (
    std::ostringstream& str, const std::vector<byte>& chunk, const char* name, size_t index
) {
    if (index + 4 >= chunk.size()) {
        std::cerr << "Constant long instruction at index " << index << " overflows the code array.";
        return index + 9;
    }

    i64 val = *(i64*)(&chunk.at(index));

    str << idStr(name) << " " << hexIntLongStr(val) << " ; " << val;

    return index + 9;
}

static size_t stringConstantInstruction (
    std::ostringstream& str,
    const BinaryBlock& block,
    const std::vector<byte>& chunk,
    const char* name,
    size_t index
) {
    if (index + 1 >= chunk.size()) {
        std::cerr << "Constant instruction at index " << index << " overflows the code array.";
        return index + 2;
    }

    byte strIndex = chunk.at(index + 1);

    str << idStr(name) << " " << hexIntStr(strIndex) << " ; " << block.stringTable.getString(strIndex);

    return index + 2;
}

static size_t stringConstantLongInstruction (
    std::ostringstream& str,
    const BinaryBlock& block,
    const std::vector<byte>& chunk,
    const char* name,
    size_t index
) {
    if (index + 4 >= chunk.size()) {
        std::cerr << "Constant long instruction at index " << index << " overflows the code array.";
        return index + 5;
    }

    i32 strIndex = *(i32*)(&chunk.at(index));

    str << idStr(name) << " " << hexIntLongStr(strIndex) << " ; " << block.stringTable.getString(strIndex);

    return index + 5;
}

static size_t jumpInstruction (std::ostringstream& str, const std::vector<byte>& chunk, const char* name, size_t index) {
    if (index + 1 >= chunk.size()) {
        std::cerr << "Jump instruction at index " << index << " overflows the code array.";
        return index + 2;
    }

    sbyte val = (sbyte)chunk.at(index + 1);

    str << idStr(name) << " " << hexIntStr(val) << " ; " << hexIntStr(index + val + 2);

    return index + 2;
}

static size_t jumpLongInstruction (std::ostringstream& str, const std::vector<byte>& chunk, const char* name, size_t index) {
    if (index + 2 >= chunk.size()) {
        std::cerr << "JumpLong instruction at index " << index << " overflows the code array.";
        return index + 5;
    }

    i32 val = *(i32*)(&chunk.at(index));

    str << idStr(name) << " " << hexIntStr(val) << " ; " << hexIntStr(index + val + 4);

    return index + 5;
}

static size_t printInstruction (std::ostringstream& str, const std::vector<byte>& chunk, const char* name, size_t index) {
    if (index + 1 >= chunk.size()) {
        std::cerr << "Print instruction at index " << index << " overflows the code array.";
        return index + 2;
    }

    byte constType = chunk.at(index + 1);

    str << idStr(name) << " " << hexIntStr(constType) << " ; " << constTypeStr(constType);

    return index + 2;
}
#pragma endregion

std::string disassembleAssembly (const AssemblyFile& file) {
    std::ostringstream str;

    str << "=== HEADER ===\n";
    str << "judith_version: " << file.judithVersion << "\n";
    str << "version: " << file.version.toString() << "\n";
    str << "\n";

    str << "=== NAME TABLE (" << sizeStr(file.nameTable.size) << ") ===\n";
    disassembleStringTable(str, file.nameTable);
    str << "\n";

    str << "=== TYPE REFERENCE TABLE ===\n";
    disassembleRefTable(str, file.typeRefs);
    str << "\n";

    str << "=== FUNCTION REFERENCE TABLE ===\n";
    disassembleRefTable(str, file.funcRefs);
    str << "\n";

    str << "=== BLOCKS (" << file.blocks.size() << ") ===\n";

    for (size_t b = 0; b < file.blocks.size(); b++) {
        str << disassembleBlock(file, b) << "\n";
    }

    return str.str();
}

static void disassembleStringTable (
    std::ostringstream& str, const StringTable& table
) {
    str << "size: " << table.size << " bytes\n";

    for (size_t i = 0; i < table.count; i++) {
        size_t offset = table.strings[i] - table.strings[0];

        str << decIntStr(i) << "   " << hexIntStr(offset) << "    \""
            << table.getString(i) << "\"\n";
    }
}

static void disassembleRefTable (std::ostringstream& str, const ItemRefTable& table) {
    for (size_t i = 0; i < table.size(); i++) {
        str << hexIntStr(i) << " ";

        if (table[i].get()->refType == ItemRef::TYPE_INTERNAL) {
            InternalRef& internalRef = *dynamic_cast<InternalRef*>(table[i].get());

            str << "(internal):\n";
            str << " - block: # " << internalRef.block << "\n";
            str << " - index: # " << internalRef.index << "\n";
        }
        else if (table[i].get()->refType == ItemRef::TYPE_NATIVE) {
            NativeRef& nativeRef = *dynamic_cast<NativeRef*>(table[i].get());

            str << "(native):\n";
            str << " - index: # " << nativeRef.index << "\n";
        }
        else if (table[i].get()->refType == ItemRef::TYPE_EXTERNAL) {
            ExternalRef& externalRef = *dynamic_cast<ExternalRef*>(table[i].get());

            str << "(external):\n";
            str << " - block_name_index: # " << externalRef.blockNameIndex << "\n";
            str << " - item_name_index: # " << externalRef.itemNameIndex << "\n";
        }
    }
}

std::string disassembleBlock (const AssemblyFile& file, size_t index) {
    auto& block = file.blocks.at(index);

    std::string blockName = file.nameTable.getString(block.nameIndex);

    std::ostringstream str;

    str << "== Block # " << hexIntStr(index) << "==\n";
    str << "name: " << block.nameIndex << " ; " << blockName;
    str << "\n";

    str << "== String table (" << sizeStr(block.stringTable.size) << ") ==\n";
    disassembleStringTable(str, block.stringTable);
    str << "\n";

    str << "== Types ==\n";
    // TODO.
    str << "\n";

    str << "== Functions ==\n";

    for (size_t f = 0; f < block.functionTable.size(); f++) {
        str << disassembleFunction(file, block, f);
    }

    str << "\n";

    return str.str();
}

static std::string disassembleFunction (
    const AssemblyFile& file, const BinaryBlock& block, size_t index
) {
    auto& func = block.functionTable.at(index);

    std::string funcName = block.stringTable.getString(func.nameIndex);

    std::ostringstream str;

    str << "= Function # " << hexIntStr(index) << " =\n";
    str << "name: " << func.nameIndex << " ; " << funcName << "\n";
    str << "max_locals: " << func.maxLocals << "\n";
    str << "max_stack: " << func.maxStack << "\n";

    str << "parameters (" << func.parameters.size() << "):\n";
    for (size_t p = 0; p < func.parameters.size(); p++) {
        disassembleParameter(str, block, func, p);
    }
    str << "\n";

    str << "chunk:\n";
    str << disassembleChunk(block, func.chunk);
    str << "\n";

    return str.str();
}

static void disassembleParameter (
    std::ostringstream& str,
    const BinaryBlock& block,
    const BinaryFunction& func,
    size_t index
) {
    auto& param = func.parameters.at(index);
    std::string paramName = block.stringTable.getString(param.nameIndex);

    str << decIntStr(index) << " - name: " << param.nameIndex << " ; " << paramName << "\n";
}

static std::string disassembleChunk (
    const BinaryBlock& block, const std::vector<byte>& chunk
) {
    std::ostringstream str;

    size_t index = 0;
    while (index < chunk.size()) {
        index = disassembleInstruction(str, block, chunk, index);
        str << "\n";
    }
    str << "\n";

    return str.str();
}

static size_t disassembleInstruction (
    std::ostringstream& str,
    const BinaryBlock& block,
    const std::vector<byte>& chunk,
    size_t index
) {
    int opCode = chunk.at(index);

    str << "Line " << std::setw(5) << std::left << -10 << std::setw(0) << " | ";
    str << hexIntStr(index) << " ";

    switch (opCode) {
    case OpCode::NOOP:
        return simpleInstruction(str, "NOOP", index);

    case OpCode::CONST:
        return constantInstruction(str, chunk, "CONST", index);
    case OpCode::CONST_L:
        return constantLongInstruction(str, chunk, "CONST_L", index);
    case OpCode::CONST_L_L:
        return constantLongLongInstruction(str, chunk, "CONST_LL", index);
    case OpCode::CONST_0:
        return simpleInstruction(str, "CONST_0", index);
    case OpCode::F_CONST_1:
        return simpleInstruction(str, "F_CONST_1", index);
    case OpCode::F_CONST_2:
        return simpleInstruction(str, "F_CONST_2", index);
    case OpCode::I_CONST_1:
        return simpleInstruction(str, "I_CONST_1", index);
    case OpCode::I_CONST_2:
        return simpleInstruction(str, "I_CONST_2", index);
    case OpCode::STR_CONST:
        return stringConstantInstruction(str, block, chunk, "STR_CONST", index);
    case OpCode::STR_CONST_L:
        return stringConstantLongInstruction(str, block, chunk, "STR_CONST_L", index);

    case OpCode::RET:
        return simpleInstruction(str, "RET", index);
    case OpCode::F_NEG:
        return simpleInstruction(str, "F_NEG", index);
    case OpCode::F_ADD:
        return simpleInstruction(str, "F_ADD", index);
    case OpCode::F_SUB:
        return simpleInstruction(str, "F_SUB", index);
    case OpCode::F_MUL:
        return simpleInstruction(str, "F_MUL", index);
    case OpCode::F_DIV:
        return simpleInstruction(str, "F_DIV", index);
    case OpCode::F_GT:
        return simpleInstruction(str, "F_GT", index);
    case OpCode::F_GE:
        return simpleInstruction(str, "F_GE", index);
    case OpCode::F_LT:
        return simpleInstruction(str, "F_LT", index);
    case OpCode::F_LE:
        return simpleInstruction(str, "F_LE", index);

    case OpCode::I_NEG:
        return simpleInstruction(str, "I_NEG", index);
    case OpCode::I_ADD:
        return simpleInstruction(str, "I_ADD", index);
    case OpCode::I_ADD_CHECKED:
        return simpleInstruction(str, "I_ADD_CHECKED", index);
    case OpCode::I_SUB:
        return simpleInstruction(str, "I_SUB", index);
    case OpCode::I_SUB_CHECKED:
        return simpleInstruction(str, "I_SUB_CHECKED", index);
    case OpCode::I_MUL:
        return simpleInstruction(str, "I_MUL", index);
    case OpCode::I_MUL_CHECKED:
        return simpleInstruction(str, "I_MUL_CHECKED", index);
    case OpCode::I_DIV:
        return simpleInstruction(str, "I_DIV", index);
    case OpCode::I_DIV_CHECKED:
        return simpleInstruction(str, "I_DIV_CHECKED", index);
    case OpCode::I_GT:
        return simpleInstruction(str, "I_GT", index);
    case OpCode::I_GE:
        return simpleInstruction(str, "I_GE", index);
    case OpCode::I_LT:
        return simpleInstruction(str, "I_LT", index);
    case OpCode::I_LE:
        return simpleInstruction(str, "I_LE", index);

    case OpCode::EQ:
        return simpleInstruction(str, "EQ", index);
    case OpCode::NEQ:
        return simpleInstruction(str, "NEQ", index);

    case OpCode::STORE_0:
        return simpleInstruction(str, "STORE_0", index);
    case OpCode::STORE_1:
        return simpleInstruction(str, "STORE_1", index);
    case OpCode::STORE_2:
        return simpleInstruction(str, "STORE_2", index);
    case OpCode::STORE_3:
        return simpleInstruction(str, "STORE_3", index);
    case OpCode::STORE_4:
        return simpleInstruction(str, "STORE_4", index);
    case OpCode::STORE:
        return byteInstruction(str, chunk, "STORE", index);
    case OpCode::STORE_L:
        return u16Instruction(str, chunk, "STORE_L", index);

    case OpCode::LOAD_0:
        return simpleInstruction(str, "LOAD_0", index);
    case OpCode::LOAD_1:
        return simpleInstruction(str, "LOAD_1", index);
    case OpCode::LOAD_2:
        return simpleInstruction(str, "LOAD_2", index);
    case OpCode::LOAD_3:
        return simpleInstruction(str, "LOAD_3", index);
    case OpCode::LOAD_4:
        return simpleInstruction(str, "LOAD_4", index);
    case OpCode::LOAD:
        return byteInstruction(str, chunk, "LOAD", index);

    case OpCode::POP:
        return simpleInstruction(str, "POP", index);

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