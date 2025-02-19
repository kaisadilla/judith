#include "executable/Chunk.hpp"

Chunk::Chunk(
    void** constants,
    byte* constantTypes,
    size_t size,
    u_ptr<byte[]> code,
    bool containsLines,
    u_ptr<i32[]> lines
) :
    constants(constants),
    constantTypes(constantTypes),
    size(size),
    code(std::move(code)),
    containsLines(containsLines),
    lines(std::move(lines))
{}
