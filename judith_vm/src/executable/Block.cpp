#include "executable/Block.hpp"

Block::Block(
    u_ptr<byte[]> constantTable,
    size_t constantCount,
    u_ptr<void*[]> constants,
    u_ptr<byte[]> constantTypes,
    Function* functions,
    size_t functionCount
) :
    constantTable(std::move(constantTable)),
    constantCount(constantCount),
    constants(std::move(constants)),
    constantTypes(std::move(constantTypes)),
    functions(functions),
    functionCount(functionCount)
{}

Block::~Block() {
    std::free(functions);
}
