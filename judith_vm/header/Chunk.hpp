#pragma once

#include "root.hpp"
#include "Value.hpp"

struct Chunk {
    size_t constantCount;
    Value* constants;

    size_t size;
    byte* code;

    bool containsLines;
    i32* lines;

    Chunk(size_t constantCount, Value* constants, size_t size, byte* code, bool containsLines, i32* lines);
    ~Chunk();
};