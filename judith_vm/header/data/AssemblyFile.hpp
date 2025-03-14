#pragma once

#include "root.hpp"
#include "StringTable.hpp"
#include "data/ItemRef.hpp"

using ItemRefTable = std::vector<u_ptr<ItemRef>>;

struct BinaryParameter {
    size_t nameIndex;
};

struct BinaryFunction {
    size_t nameIndex;
    std::vector<BinaryParameter> parameters;
    i64 maxLocals;
    i64 maxStack;
    std::vector<byte> chunk;
    size_t chunkSize;
};

struct BinaryBlock {
    size_t nameIndex;
    StringTable stringTable;
    std::vector<BinaryFunction> functionTable;
};

struct AssemblyFile {
    i64 judithVersion;
    Version version;
    StringTable nameTable;
    ItemRefTable typeRefs;
    ItemRefTable funcRefs;
    std::vector<BinaryBlock> blocks;

    static AssemblyFile loadFromFile (const char* path);
};