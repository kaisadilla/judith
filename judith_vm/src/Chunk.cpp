#include "Chunk.hpp"

Chunk::Chunk(
    byte* constantTable,
    size_t constantCount,
    void** constants,
    size_t size,
    byte* code,
    bool containsLines,
    i32* lines
) :
    constantTable(constantTable),
    constantCount(constantCount),
    constants(constants),
    size(size),
    code(code),
    containsLines(containsLines),
    lines(lines)
{}

Chunk::~Chunk () {
    delete[] constants;
    delete[] code;
    delete[] lines;
}
/*
    size_t constantCount;
    Value* constants;

    size_t size;
    byte* code;

    bool containsLines;
    i32* lines;*/