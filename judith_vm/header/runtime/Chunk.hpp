#pragma once

#include "root.hpp"

struct Chunk {
    /// <summary>
    /// A pointer to the constant pointer array of the Block that contains this
    /// Chunk.
    /// </summary>
    byte** strings;

    size_t size;
    u_ptr<byte[]> code;

    bool containsLines;
    u_ptr<i32[]> lines;

    Chunk(
        byte** strings,
        size_t size,
        u_ptr<byte[]> code,
        bool containsLines,
        u_ptr<i32[]> lines
    );
};
