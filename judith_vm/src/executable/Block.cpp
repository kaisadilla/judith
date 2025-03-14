#include "executable/Block.hpp"

Block::Block(
    u_ptr<byte[]> stringTable,
    size_t stringCount,
    u_ptr<byte*[]> strings,
    VmFunc* functions,
    size_t functionCount
) :
    stringTable(std::move(stringTable)),
    stringCount(stringCount),
    strings(std::move(strings)),
    functions(functions),
    functionCount(functionCount)
{}

Block::~Block() {
    std::free(functions);
}
