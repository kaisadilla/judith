#include "Chunk.hpp"

Chunk::Chunk(
    byte* constantTable,
    size_t constantCount,
    void** constants,
    byte* constantTypes,
    size_t size,
    byte* code,
    bool containsLines,
    i32* lines
) :
    constantTable(constantTable),
    constantCount(constantCount),
    constants(constants),
    constantTypes(constantTypes),
    size(size),
    code(code),
    containsLines(containsLines),
    lines(lines)
{}

Chunk::~Chunk () {
    delete[] constantTable;
    delete[] constants;
    delete[] constantTypes;
    delete[] code;
    if (containsLines) {
        delete[] lines;
    }
}
/*
    size_t constantCount;
    Value* constants;

    size_t size;
    byte* code;

    bool containsLines;
    i32* lines;*/