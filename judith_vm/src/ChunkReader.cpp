#include "ChunkReader.hpp"
#include "utils.hpp"
#include <utils/Buffer.hpp>

#define CHECK_SIZE(n) \
    do { \
        if ((offset + (n)) >= bufferSize) { \
            throw std::runtime_error("Constant overflows the file!"); \
        } \
    } while(false)

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

Chunk readChunk() {
    constexpr size_t MAGIC_NUMBER_COUNT = 12;

    auto buffer = readBinaryFile("res/test.jbin");
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

    ui32 constantCount = reader.readUInt32_LE(); // constant_count

    std::vector<byte> alignedConstantVector;
    std::vector<size_t> cIndices;
    std::vector<byte> cTypes;

    for (int i = 0; i < constantCount; i++) {
        // Read the type of the next constant in the file's table.
        byte type = reader.readUInt8();
        // Get the byte offset we are in the file (after reading the type byte).
        size_t offset = reader.getReadOffset();
        // This constant's index in alignedConstantVector is its current size.
        cIndices.push_back(alignedConstantVector.size());
        cTypes.push_back(type);

        // We'll always check that the file is big enough to contain the bytes
        // we will read next with CHECK_SIZE(n). This prevents corrupt or malicious
        // files from stepping into bad memory. The current implementation of
        // buffer returns 0 in this case, but we still check here.
        switch (type) {
        case ConstantType::ERROR:
            throw std::runtime_error("ERROR type found in the constant table!");

        case ConstantType::INT_64:
        case ConstantType::FLOAT_64:
        case ConstantType::UNSIGNED_INT_64:
            CHECK_SIZE(8);

            // Push the next 8 bytes in the file.
            for (int i = 0; i < 8; i++) {
                alignedConstantVector.push_back(reader.readUInt8());
            }

            break;
        case ConstantType::STRING_ASCII: {
            // Read the size, in bytes, of the string.
            size_t stringSize = reader.readUInt64_LE();
            offset = reader.getReadOffset();
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

            break;
        }
        default:
            throw std::runtime_error("Unknown type found in the constant table.");
        }
    }

    // Create a persistent array for the table.
    byte* constantTable = new byte[alignedConstantVector.size()];
    // Copy the constant table there.
    std::copy(alignedConstantVector.begin(), alignedConstantVector.end(), constantTable);

    // Create a persistent array for the types.
    byte* constantTypes = new byte[cTypes.size()];
    std::copy(cTypes.begin(), cTypes.end(), constantTypes);

    // This array will map each index in the file's constant table to an address
    // within constantTable, so values can be looked up directly.
    void** constants = new void* [cIndices.size()];

    for (size_t i = 0; i < cIndices.size(); i++) {
        // The element #i in the file's constant table is at constantTable[index].
        size_t index = cIndices[i];
        // the address of constantTable[index] becomes the pointer to the value.
        constants[i] = (void*)(&constantTable[index]);
    }

    reader.readUInt32_LE(); // Discard function_count, right now we only accept one function.

    ui32 size = reader.readUInt32_LE();
    byte* code = new byte[size];
    
    for (int i = 0; i < size; i++) {
        code[i] = reader.readUInt8();
    }

    bool containsLines = reader.readBool();
    i32* lines = nullptr;
    if (containsLines) {
        lines = new i32[size];

        for (int i = 0; i < size; i++) {
            lines[i] = reader.readInt32_LE();
        }
    }

    reader.readInt32_LE(); // discard entry_point - we assume the only function is.

    return Chunk(
        constantTable,
        constantCount,
        constants,
        constantTypes,
        size,
        code,
        containsLines,
        lines
    );
}
