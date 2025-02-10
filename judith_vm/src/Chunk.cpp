#include "Chunk.hpp"

Chunk::Chunk(
    size_t constantCount,
    Value* constants,
    size_t size,
    byte* code,
    bool containsLines,
    i32* lines
) :
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