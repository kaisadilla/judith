#pragma once

#include "root.hpp"
#include "runtime/JasmFunction.hpp"
#include "runtime/object/StringObject.hpp"
#include "data/AssemblyFile.hpp"

class VM;
class Assembly;

class Block {
public:
    /// <summary>
    /// Points to the assembly that contains this block.
    /// </summary>
    Assembly* assembly;
    /// <summary>
    /// Points to the string object that contains this block's name.
    /// </summary>
    StringObject* name;

    /// <summary>
    /// Every string in a block's string table is interned when the block is
    /// loaded. This table maps each index to the interned StringObject that
    /// contains the string in the table.
    /// </summary>
    std::vector<StringObject*> stringTable;

    std::vector<u_ptr<JasmFunction>> functions;

public:
    static u_ptr<Block> buildCompletely (VM& vm, Assembly* assembly, const BinaryBlock& binaryBlock);
};
