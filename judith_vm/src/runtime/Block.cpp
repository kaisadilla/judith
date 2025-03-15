#include "runtime/Block.hpp"
#include "runtime/Assembly.hpp"
#include "runtime/JasmFunction.hpp"
#include "VM.hpp"

u_ptr<Block> Block::buildCompletely(
    VM& vm, Assembly* assembly, const BinaryBlock& binaryBlock
) {
    u_ptr<Block> block = make_u<Block>(Block {
        .assembly = assembly,
    });

    block->name = assembly->nameTable.at(binaryBlock.nameIndex);

    for (size_t i = 0; i < binaryBlock.stringTable.count; i++) {
        block->stringTable.push_back(
            vm.getInternedStringTable().getStringObject(
                binaryBlock.stringTable.strings[i]
            )
        );
    }

    for (size_t f = 0; f < binaryBlock.functionTable.size(); f++) {
        block->functions.emplace_back(
            JasmFunction::build(
                vm, *assembly, *block, binaryBlock.functionTable.at(f)
            )
        );
    }

    return std::move(block);
}
