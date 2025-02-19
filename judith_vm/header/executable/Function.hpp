#pragma once

#include "root.hpp"
#include "executable/Chunk.hpp"

struct Function {
    Chunk chunk;

    Function (Chunk&& chunk) : chunk(std::move(chunk)) {}
};