#pragma once

#include "root.hpp"

struct Chunk {
    /// <summary>
    /// A pointer to the constant pointer array of the Block that contains this
    /// Chunk.
    /// </summary>
    void** constants;
    /// <summary>
    /// A pointer to the constant type array of the Block that contains this
    /// Chunk.
    /// </summary>
    byte* constantTypes;

    size_t size;
    u_ptr<byte[]> code;

    bool containsLines;
    u_ptr<i32[]> lines;

    Chunk(
        void** constants,
        byte* constantTypes,
        size_t size,
        u_ptr<byte[]> code,
        bool containsLines,
        u_ptr<i32[]> lines
    );
};
