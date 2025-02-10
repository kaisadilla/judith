#pragma once

#include "root.hpp"
#include "Chunk.hpp"

enum class InterpretResult {
    OK,
    RUNTIME_ERROR,
};

class VM {
public:
    InterpretResult interpret (const Chunk& chunk);
};