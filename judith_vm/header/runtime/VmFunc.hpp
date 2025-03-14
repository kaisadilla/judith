#pragma once

#include "root.hpp"
#include "runtime/Chunk.hpp"

struct VmFunc {
    size_t maxLocals;
    Chunk chunk;

    VmFunc (size_t maxLocals, Chunk&& chunk)
        : maxLocals(maxLocals),
        chunk(std::move(chunk))
    {}
};

struct FunctionRef {
    size_t block;
    size_t index;

    FunctionRef (size_t block, size_t index) : block(block), index(index) {}
};