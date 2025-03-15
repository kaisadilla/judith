#pragma once

#include "root.hpp"
#include "runtime/Block.hpp"
#include "runtime/object/StringObject.hpp"
#include "runtime/FunctionCollection.hpp"
#include "data/StringTable.hpp"
#include "data/AssemblyFile.hpp"

class VM;

class Assembly {
public:
    /// <summary>
    /// When the assembly is loaded, all of the strings in its name table are
    /// interned by the VM that loaded it. This vector maps each index in the
    /// name table to the interned StringObject that contains it.
    /// </summary>
    std::vector<StringObject*> nameTable;

    std::vector<u_ptr<Block>> blocks;

    FunctionCollection functionRefs;

    //std::vector<Block> blocks;
    //FunctionCollection funcRefs;

public:
    Assembly (size_t funcRefCount)
        : functionRefs(FunctionCollection(funcRefCount))
    {}

    //Assembly (const Assembly&) = delete;
    //Assembly& operator= (const Assembly&) = delete;

    //Assembly (Assembly&& other) noexcept
    //    : nameTable(std::move(other.nameTable)),
    //    blocks(std::move(other.blocks)),
    //    functionRefs(std::move(other.functionRefs))
    //{
    //    bindReferences();
    //}

    //Assembly& operator= (Assembly&& other) noexcept {
    //    if (this != &other) {
    //        nameTable = std::move(other.nameTable);
    //        blocks = std::move(other.blocks);
    //        functionRefs = std::move(other.functionRefs);
    //
    //        bindReferences();
    //    }
    //
    //    return *this;
    //}

public:
    static u_ptr<Assembly> buildCompletely (VM& vm, AssemblyFile& file);

    /// <summary>
    /// Binds all weak pointers and references inside the objects that make up
    /// the assembly.
    /// </summary>
    void bindReferences();
};
