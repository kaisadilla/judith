#pragma once

#include "root.hpp"

union Value;

struct Chunk {
    //size_t constantCount;
    //Value* constants;
    
    /// <summary>
    /// A pointer to the start of the raw constant table.
    /// </summary>
    byte* constantTable;
    /// <summary>
    /// The size (amount of bytes) in the constant table.
    /// </summary>
    size_t constantCount;
    /// <summary>
    /// An array where each element is a pointer to the constant at that index
    /// in the table.
    /// </summary>
    void** constants;

    size_t size;
    byte* code;

    bool containsLines;
    i32* lines;

    Chunk(byte* constantTable, size_t constantCount, void** constants, size_t size, byte* code, bool containsLines, i32* lines);
    ~Chunk();
};

namespace ConstantType {
    enum {
        ERROR = 0,
        INT_64,
        FLOAT_64,
        UNSIGNED_INT_64,
        STRING_ASCII,
    };
}