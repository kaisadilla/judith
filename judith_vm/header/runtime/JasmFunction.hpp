#pragma once

#include "root.hpp"
#include "runtime/Chunk.hpp"
#include "runtime/object/StringObject.hpp"
#include "data/AssemblyFile.hpp"

class VM;
class Assembly;
class Block;
class FunctionCollection;

struct JasmParameter {
    StringObject* name;
};

class JasmFunction {
public:
    FunctionCollection* funcRefs;
    StringObject* name;
    std::vector<JasmParameter> parameters;
    size_t maxLocals;
    size_t maxStack;
    Chunk chunk;

    static u_ptr<JasmFunction> build (VM& vm, Assembly& assembly, Block& block, const BinaryFunction& binaryFunc);
};
