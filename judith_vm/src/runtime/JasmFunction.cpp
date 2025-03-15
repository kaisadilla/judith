#include "runtime/JasmFunction.hpp"
#include "VM.hpp";
#include "runtime/Block.hpp"

u_ptr<JasmFunction> JasmFunction::build (
    VM& vm, Assembly& assembly, Block& block, const BinaryFunction& binaryFunc
) {
    u_ptr<JasmFunction> func = make_u<JasmFunction>(JasmFunction{
        .funcRefs = &assembly.functionRefs,
        .chunk = std::move(*Chunk::build(block, binaryFunc.chunk)),
    });

    func->name = block.stringTable.at(binaryFunc.nameIndex);
    func->maxLocals = binaryFunc.maxLocals;
    func->maxStack = binaryFunc.maxStack;

    for (auto& binaryParam : binaryFunc.parameters) {
        func->parameters.emplace_back(JasmParameter{
            .name = block.stringTable.at(binaryParam.nameIndex),
        });
    }

    return std::move(func);
}
