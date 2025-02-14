#include "ChunkReader.hpp"
#include "utils.hpp"
#include <utils/Buffer.hpp>
#include "Value.hpp"

#define CHECK_SIZE(n) \
    do { \
        if ((offset + (n)) >= bufferSize) throw "Constant overflows the file!"; \
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
        std::cout << "Expected magic number not found." << std::endl;
    }

    i32 constantCount = reader.readInt32_LE();

    std::vector<byte> alignedConstantVector;
    std::vector<size_t> indices;

    for (int i = 0; i < constantCount; i++) {
        // Read the type of the next constant in the file's table.
        byte type = reader.readUInt8();
        // Get the byte offset we are in the file (after reading the type byte).
        size_t offset = reader.getReadOffset();
        // This constant's index in alignedConstantVector is its current size.
        indices.push_back(alignedConstantVector.size());

        // We'll always check that the file is big enough to contain the bytes
        // we will read next with CHECK_SIZE(n). This prevents corrupt or malicious
        // files from stepping into bad memory. The current implementation of
        // buffer returns 0 in this case, but we still check here.
        switch (type) {
        case ConstantType::ERROR:
            throw "ERROR type found in the constant table!";

        case ConstantType::INT_64:
        case ConstantType::FLOAT_64:
        case ConstantType::UNSIGNED_INT_64:
            CHECK_SIZE(8);

            // Push the next 8 bytes in the file.
            for (int i = 0; i < 8; i++) {
                alignedConstantVector.push_back(reader.readUInt8());
            }

            break;
        case ConstantType::STRING_ASCII:
            // Read the size, in bytes, of the string.
            size_t stringSize = reader.readUInt8();
            CHECK_SIZE(stringSize + 1); // +1 because offset hasn't been updated.

            // Push all the bytes in the string.
            for (int i = 0; i < stringSize < 8; i++) {
                alignedConstantVector.push_back(reader.readUInt8());
            }

            // Pad the end of the value to preserve alignment.
            while (alignedConstantVector.size() % 8 != 0) {
                alignedConstantVector.push_back(0);
            }

            break;
        default:
            throw "Unknown type found in the constant table.";
        }
    }

    // Create a persistent array.
    byte* constantTable = new byte[alignedConstantVector.size()];
    // Copy the constant table there.
    std::copy(alignedConstantVector.begin(), alignedConstantVector.end(), constantTable);
    // This array will map each index in the file's constant table to an address
    // within constantTable, so values can be looked up directly.
    void** constants = new void* [indices.size()];

    for (size_t i = 0; i < indices.size(); i++) {
        // The element #i in the file's constant table is at constantTable[index].
        size_t index = indices[i];
        // the address of constantTable[index] becomes the pointer to the value.
        constants[i] = (&constantTable[index]);
    }

    i32 size = reader.readInt32_LE();
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

    return Chunk(
        constantCount, constants, size, code, containsLines, lines
    );
}
