#include "data/StringTable.hpp"
#include "utils/Buffer.hpp"

u_ptr<StringTable> StringTable::fromBinary(std::vector<byte> block) {
    size_t count = *(size_t*)&block[0];
    throw "Not implemented.";

    //Buffer buffer((block));
    //
    //std::vector<size_t> offsets; // maps each index to its offset in the table. 
    //
    //size_t count = (size_t)buffer.readUInt32_LE(); // string_count: Ui32
    //
    //for (size_t i = 0; i < count; i++) { // strings: String[]
    //    offsets.push_back(tableBytes.size()); // The current size of the table becomes the index for this string.
    //
    //    size_t stringSize = (size_t)buffer.readUInt64_LE(); // size (the size in bytes of the following string).
    //
    //    // Insert the bytes that make up 'stringSize' into the table.
    //    byte* sizePtr = (byte*)&stringSize;
    //    tableBytes.insert(tableBytes.end(), sizePtr, sizePtr + sizeof(size_t));
    //
    //    // Push all the bytes that make up the string into the table.
    //    for (size_t j = 0; j < stringSize; j++) {
    //        tableBytes.push_back(buffer.readUInt8());
    //    }
    //
    //    // Pad the end of the value to preserve alignment.
    //    // TODO: This padding is for x64, other architectures may need different
    //    // padding or no padding at all.
    //    while (tableBytes.size() % 8 != 0) {
    //        tableBytes.push_back(0);
    //    }
    //}
    //
    //// Create a raw table from the vector of bytes.
    //u_ptr<byte[]> rawTable = make_u<byte[]>(tableBytes.size());
    //std::copy(tableBytes.begin(), tableBytes.end(), rawTable.get());
    //
    //u_ptr<byte* []> strings = make_u<byte * []>(count);
    //
    //for (size_t i = 0; i < count; i++) {
    //    // The offset of string #i in the table.
    //    size_t offset = offsets[i];
    //
    //    // The offset's address becomes the pointer to the string.
    //    strings[i] = &rawTable[offset];
    //}
    //
    //
    //return make_u<StringTable>(
    //    std::move(rawTable), tableBytes.size(), std::move(strings), count
    //);
}
