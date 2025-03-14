#include "loader/AssemblyLoader.hpp"
#include "executable/Assembly.hpp"
#include "utils.hpp"
#include <utils/Buffer.hpp>
#include <executable/StringTable.hpp>
#include "executable/ItemRef.hpp"

using ItemRefVector = std::vector<u_ptr<ItemRef>>;

static bool checkMagicNumber(byte* magicNumber);
static Version readVersion(Buffer& buffer);
static u_ptr<StringTable> readStringTable(Buffer& buffer);
static ItemRefVector readItemRefTable(Buffer& buffer);

Assembly readAssembly(const char* path) {
    constexpr size_t MAGIC_NUMBER_SIZE = 6;

    auto binary = readBinaryFile(path);
    size_t bufferSize = binary.size();
    Buffer buffer((binary));

    // magic_number: Byte[6] (expected: 'JUDITH').
    byte magicNumber[MAGIC_NUMBER_SIZE]{};
    for (size_t i = 0; i < MAGIC_NUMBER_SIZE; i++) {
        magicNumber[i] = buffer.readUInt8();
    }

    if (checkMagicNumber(magicNumber)) {
        throw std::runtime_error("Invalid magic number.");
    }

    buffer.readUInt8(); // discard 'endianness'
    buffer.readUInt32_LE(); // discard 'judith_version'
    readVersion(buffer); // discard 'version'

    u_ptr<StringTable> nameTable = readStringTable(buffer); // name_count and name_table.

    buffer.readUInt32_LE(); // discard 'dep_count' (which right now is always 0).

    return Assembly(std::move(nameTable));
}

static bool checkMagicNumber (byte* magicNumber) {
    return magicNumber[0] == 'J'
        && magicNumber[1] == 'U'
        && magicNumber[1] == 'D'
        && magicNumber[1] == 'I'
        && magicNumber[1] == 'T'
        && magicNumber[1] == 'H';
}

static Version readVersion (Buffer& buffer) {
    return {
        .major = buffer.readUInt16_LE(),
        .minor = buffer.readUInt16_LE(),
        .patch = buffer.readUInt16_LE(),
        .build = buffer.readUInt16_LE(),
    };
}

static u_ptr<StringTable> readStringTable (Buffer& buffer) {
    std::vector<byte> tableBytes; // contains all the bytes that make up the table.
    std::vector<size_t> offsets; // maps each index to its offset in the table. 

    size_t count = (size_t)buffer.readUInt32_LE(); // string_count: Ui32

    for (size_t i = 0; i < count; i++) { // strings: String[]
        offsets.push_back(tableBytes.size()); // The current size of the table becomes the index for this string.

        size_t stringSize = (size_t)buffer.readUInt64_LE(); // size (the size in bytes of the following string).

        // Insert the bytes that make up 'stringSize' into the table.
        byte* sizePtr = (byte*)&stringSize;
        tableBytes.insert(tableBytes.end(), sizePtr, sizePtr + sizeof(size_t));

        // Push all the bytes that make up the string into the table.
        for (size_t j = 0; j < stringSize; j++) {
            tableBytes.push_back(buffer.readUInt8());
        }

        // Pad the end of the value to preserve alignment.
        // TODO: This padding is for x64, other architectures may need different
        // padding or no padding at all.
        while (tableBytes.size() % 8 != 0) {
            tableBytes.push_back(0);
        }
    }

    // Create a raw table from the vector of bytes.
    u_ptr<byte[]> rawTable = make_u<byte[]>(tableBytes.size());
    std::copy(tableBytes.begin(), tableBytes.end(), rawTable.get());

    u_ptr<byte*[]> strings = make_u<byte*[]>(count);

    for (size_t i = 0; i < count; i++) {
        // The offset of string #i in the table.
        size_t offset = offsets[i];

        // The offset's address becomes the pointer to the string.
        strings[i] = &rawTable[offset];
    }


    return make_u<StringTable>(
        std::move(rawTable), tableBytes.size(), std::move(strings), count
    );
}

ItemRefVector readItemRefTable (Buffer& buffer) {

}