#include "runtime/Chunk.hpp"

Chunk::Chunk(
    byte** strings,
    size_t size,
    u_ptr<byte[]> code,
    bool containsLines,
    u_ptr<i32[]> lines
) :
    strings(strings),
    size(size),
    code(std::move(code)),
    containsLines(containsLines),
    lines(std::move(lines))
{}
