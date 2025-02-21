#pragma once

#include "root.hpp"
#include "executable/Block.hpp"
#include "executable/Function.hpp"

class Assembly {
public:
    OWNED Function** assemblyFunctions;
    size_t functionCount;

    std::vector<Block> blocks;

public:
    Assembly(
        Function** assemblyFunctions,
        size_t functionCount,
        std::vector<Block>&& blocks
    ) :
        assemblyFunctions(assemblyFunctions),
        functionCount(functionCount),
        blocks(std::move(blocks)) {}

    ~Assembly() {
        delete[] assemblyFunctions;
    }

};