#include "BlockReader.hpp"
#include "utils.hpp"
#include <utils/Buffer.hpp>
#include "executable/Assembly.hpp"
#include "executable/Block.hpp"
#include "executable/Function.hpp"
#include "executable/ConstantType.hpp"

#define CHECK_SIZE(n) \
    do { \
        if ((offset + (n)) >= bufferSize) { \
            throw std::runtime_error("Constant overflows the file!"); \
        } \
    } while(false)

static std::vector<FunctionRef> readFuncRefTable (Buffer& reader, size_t bufferSize);
static void readBlock(Buffer& reader, size_t bufferSize, std::vector<Block>&blocks);

void readStringTable (
    Buffer& reader,
    size_t bufferSize,
    u_ptr<byte[]>& stringTable,
    size_t & stringCount,
    u_ptr<byte*[]>& strings
);

void readFunctionTable (
    Buffer& reader,
    size_t bufferSize,
    const u_ptr<byte*[]>& strings,
    Function*& functions,
    size_t & functionCount
);

bool isMagicNumberCorrect (byte* magicNumbers) {
    return magicNumbers[0] == 'A'
        && magicNumbers[1] == 'Z'
        && magicNumbers[2] == 'A'
        && magicNumbers[3] == 'R'
        && magicNumbers[4] == 'I'
        && magicNumbers[5] == 'A'
        && magicNumbers[6] == 'J'
        && magicNumbers[7] == 'U'
        && magicNumbers[8] == 'D'
        && magicNumbers[9] == 'I'
        && magicNumbers[10] == 'T'
        && magicNumbers[11] == 'H';
}

Assembly readAssembly () {
    constexpr size_t MAGIC_NUMBER_COUNT = 12;

    std::cout << "JuVM C++" << std::endl;

    auto buffer = readBinaryFile("res/test.jdll");
    size_t bufferSize = buffer.size();
    Buffer reader((buffer));

    byte magicNumbers[MAGIC_NUMBER_COUNT]{};
    for (int i = 0; i < MAGIC_NUMBER_COUNT; i++) {
        magicNumbers[i] = reader.readUInt8();
    }

    if (isMagicNumberCorrect(magicNumbers) == false) {
        throw std::runtime_error("Constant overflows the file!");
    }

    reader.readUInt8(); // discard endianness
    reader.readUInt8(); // discard major_version
    reader.readUInt8(); // discard minor_version

    std::vector<FunctionRef> funcRefs = readFuncRefTable(reader, bufferSize);
    // eagerly loading functions.

    std::vector<Block> blocks;
    size_t blockCount = reader.readUInt32_LE(); // block_count

    for (size_t i = 0; i < blockCount; i++) { // blocks
        readBlock(reader, bufferSize, blocks);
    }

    Function** functions = new Function*[funcRefs.size()];
    for (size_t i = 0; i < funcRefs.size(); i++) {
        functions[i] = &(blocks[funcRefs[i].block].functions[funcRefs[i].index]);
    }

    return Assembly(functions, funcRefs.size(), std::move(blocks));
}

static std::vector<FunctionRef> readFuncRefTable (Buffer& reader, size_t bufferSize) {
    std::vector<FunctionRef> refs;

    size_t refCount = (size_t)reader.readUInt32_LE(); // ref_count

    for (size_t i = 0; i < refCount; i++) { // func_refs
        size_t block = (size_t)reader.readUInt32_LE(); // block
        size_t index = (size_t)reader.readUInt32_LE(); // index

        refs.push_back(FunctionRef(block, index));
    }

    return refs;
}

static void readBlock(Buffer& reader, size_t bufferSize, std::vector<Block>& blocks) {
    u_ptr<byte[]> stringTable;
    size_t stringCount;
    u_ptr<byte*[]> strings;

    // Read constant table and output it to the variables above.
    readStringTable(reader, bufferSize, stringTable, stringCount, strings);

    bool hasImplicitFunc = reader.readBool(); // has_implicit

    Function* functions;
    size_t functionCount;

    readFunctionTable(reader, bufferSize, strings, functions, functionCount);

    blocks.emplace_back(
        std::move(stringTable),
        stringCount,
        std::move(strings),
        functions,
        functionCount
    );
}

void readStringTable (
    Buffer& reader,
    size_t bufferSize,
    u_ptr<byte[]>& stringTable,
    size_t& stringCount,
    u_ptr<byte*[]>& strings
) {
    stringCount = (size_t)reader.readUInt32_LE(); // string_count

    std::vector<byte> alignedConstantVector;
    std::vector<size_t> cIndices;

    for (int i = 0; i < stringCount; i++) {
        // Read the type of the next constant in the file's table.
        ui64 stringSize = reader.readUInt64_LE();
        // Get the byte offset we are in the file (after reading the type byte).
        size_t offset = reader.getReadOffset();
        // This constant's index in alignedConstantVector is its current size.
        cIndices.push_back(alignedConstantVector.size());

        // We'll always check that the file is big enough to contain the bytes
        // we will read next with CHECK_SIZE(n). This prevents corrupt or malicious
        // files from stepping into bad memory. The current implementation of
        // buffer returns 0 in this case, but we still check here.
        CHECK_SIZE(stringSize);

        // Push string size:
        byte* sizePtr = (byte*)&stringSize;
        alignedConstantVector.insert(
            alignedConstantVector.end(), sizePtr, sizePtr + sizeof(size_t)
        );

        // Push all the bytes in the string.
        for (int i = 0; i < stringSize; i++) {
            alignedConstantVector.push_back(reader.readUInt8());
        }

        // Pad the end of the value to preserve alignment.
        while (alignedConstantVector.size() % 8 != 0) {
            alignedConstantVector.push_back(0);
        }
    }

    // Create a persistent array for the table.
    stringTable = make_u<byte[]>(alignedConstantVector.size());
    // Copy the constant table there.
    std::copy(alignedConstantVector.begin(), alignedConstantVector.end(), stringTable.get());

    // This array will map each index in the file's constant table to an address
    // within constantTable, so values can be looked up directly.
    strings = make_u<byte* []>(cIndices.size());

    for (size_t i = 0; i < cIndices.size(); i++) {
        // The element #i in the file's constant table is at constantTable[index].
        size_t index = cIndices[i];
        // the address of constantTable[index] becomes the pointer to the value.
        strings[i] = (byte*)(&stringTable[index]);
    }
}

void readFunctionTable (
    Buffer& reader,
    size_t bufferSize,
    const u_ptr<byte* []>& strings,
    Function*& functions,
    size_t& functionCount
) {
    functionCount = (size_t)reader.readUInt32_LE(); // function_count
    functions = (Function*)std::malloc(functionCount * sizeof(Function));

    for (int f = 0; f < functionCount; f++) {
        reader.readUInt32_LE(); // discard function:name
        size_t paramCount = (size_t)reader.readUInt16_LE(); // param_count

        for (int p = 0; p < paramCount; p++) {
            reader.readUInt32_LE(); // discard param:name
        }

        byte maxLocals = (byte)reader.readUInt16_LE(); // max_locals
        reader.readUInt16_LE(); // discard max_stack

        // Read chunk:

        size_t size = (size_t)reader.readUInt32_LE(); // code_bloc
        u_ptr<byte[]> code = make_u<byte[]>(size);

        for (int i = 0; i < size; i++) {
            code[i] = reader.readUInt8(); // code
        }

        bool containsLines = reader.readBool(); // has_lines
        u_ptr<i32[]> lines = nullptr;

        if (containsLines) {
            lines = make_u<i32[]>(size);

            for (int i = 0; i < size; i++) {
                lines[i] = reader.readInt32_LE(); // lines
            }
        }

        // TODO: Check this.
        new (&functions[f]) Function(maxLocals, Chunk(
            strings.get(),
            size,
            std::move(code),
            containsLines,
            std::move(lines)
        ));
    }
}