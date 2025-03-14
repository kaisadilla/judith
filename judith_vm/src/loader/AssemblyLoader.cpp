#include "loader/AssemblyLoader.hpp"
#include "runtime/Assembly.hpp"
#include "utils.hpp"
#include <utils/Buffer.hpp>
#include "data/StringTable.hpp"
#include "data/ItemRef.hpp"
#include "runtime/Block.hpp"

using ItemRefVector = std::vector<u_ptr<ItemRef>>;

static bool checkMagicNumber(byte* magicNumber);
static Version readVersion(Buffer& buffer);
static u_ptr<StringTable> readStringTable(Buffer& buffer);
static ItemRefVector readItemRefTable(Buffer& buffer);
static Block readBlock(Buffer& buffer, size_t& nameIndex);

Assembly readAssembly(VM* vm, const char* filePath) {
    constexpr size_t MAGIC_NUMBER_SIZE = 6;

    auto binary = readBinaryFile(filePath);
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

    ItemRefVector typeRefTable = readItemRefTable(buffer); // type_ref_table
    ItemRefVector funcRefTable = readItemRefTable(buffer); // func_ref_table

    size_t blockCount = buffer.readUInt32_LE(); // block_count

    std::vector<Block> blocks;
    std::vector<std::string> blockNames;

    for (size_t i = 0; i < blockCount; i++) {
        size_t nameIndex;
        readBlock(buffer, nameIndex);

        blockNames.push_back(nameTable->getString(nameIndex));
    }


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
    size_t size = (size_t)buffer.readUInt32_LE(); // table_size: Ui32

    // TODO
    //std::vector<byte> binaryBlock; // contains all the bytes that make up the table.
    //
    //for (size_t i = 0; i < size; i++) { // name_table: StringTable
    //    binaryBlock.push_back(buffer.readUInt8());
    //}
    //
    //return StringTable::fromBinary(binaryBlock);

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

static ItemRefVector readItemRefTable (Buffer& buffer) {
    ItemRefVector vector;

    size_t count = (size_t)buffer.readUInt32_LE();

    for (size_t i = 0; i < count; i++) {
        size_t type = (size_t)buffer.readUInt32_LE();

        switch (type) {
        case ItemRef::TYPE_INTERNAL: {
            size_t block = (size_t)buffer.readUInt32_LE();
            size_t index = (size_t)buffer.readUInt32_LE();

            vector.push_back(make_u<InternalRef>(block, index));
            break;
        }
        case ItemRef::TYPE_NATIVE: {
            size_t index = (size_t)buffer.readUInt32_LE();

            vector.push_back(make_u<NativeRef>(index));
            break;
        }
        case ItemRef::TYPE_EXTERNAL: {
            size_t blockNameIndex = (size_t)buffer.readUInt32_LE();
            size_t itemNameIndex = (size_t)buffer.readUInt32_LE();

            vector.push_back(make_u<ExternalRef>(blockNameIndex, itemNameIndex));
            break;
        }
        }
    }

    return vector;
}

static Block readBlock(Buffer& buffer, size_t& nameIndex) {
    nameIndex = buffer.readUInt32_LE(); // block_name

    u_ptr<StringTable> stringTable = readStringTable(buffer); // string_count and string_table

    size_t typeCount = buffer.readUInt32_LE(); // type_count: Ui32 (right now, always 0).
    // TODO: type_table

    size_t funcCount = buffer.readUInt32_LE(); // func_count: Ui32

    for (size_t f = 0; f < funcCount; f++) {

    }
    throw "E";
}