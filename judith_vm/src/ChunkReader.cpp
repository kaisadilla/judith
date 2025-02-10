#include "ChunkReader.hpp"
#include "utils.hpp"
#include <utils/Buffer.hpp>

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

std::unique_ptr<Chunk> readChunk() {
    constexpr size_t MAGIC_NUMBER_COUNT = 12;

    auto buffer = readBinaryFile("res/test.jbin");
    Buffer reader((buffer));

    byte magicNumbers[MAGIC_NUMBER_COUNT]{};
    for (int i = 0; i < MAGIC_NUMBER_COUNT; i++) {
        magicNumbers[i] = reader.readUInt8();
    }

    if (isMagicNumberCorrect(magicNumbers) == false) {
        std::cout << "Expected magic number not found." << std::endl;
    }

    i32 constantCount = reader.readInt32_LE();
    Value* constants = new Value[constantCount]();

    for (int i = 0; i < constantCount; i++) {
        constants[i].type = (ValueType)reader.readUInt8();
        constants[i].as.floatNum = (f64)reader.readDouble_LE();
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


    return std::make_unique<Chunk>(
        constantCount, constants, size, code, containsLines, lines
    );
}
