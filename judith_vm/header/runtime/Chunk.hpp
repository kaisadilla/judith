#pragma once

#include "root.hpp"
#include "runtime/object/StringObject.hpp"

class Block;

class Chunk {
public:
    /// <summary>
    /// A reference to the string table in the block this chunk is in.
    /// </summary>
    const std::vector<StringObject*>* stringTable;

    size_t size;
    u_ptr<byte[]> code;

    static u_ptr<Chunk> build (const Block& block, const std::vector<byte>& binaryChunk);
};
