#pragma once

#include "root.hpp"
#include "runtime/Block.hpp"
#include "runtime/VmFunc.hpp"
#include "runtime/FunctionCollection.hpp"
#include "data/StringTable.hpp"

class VM;

class Assembly {
public:
    StringTable nameTable;
    //std::vector<Block> blocks;
    //FunctionCollection funcRefs;

public:
    Assembly(
        u_ptr<StringTable> nameTable
        //std::vector<Block>&& blocks,
        //FunctionCollection&& funcRefs
    ) :
        nameTable(std::move(*nameTable))
        //blocks(std::move(blocks)),
        //funcRefs(funcRefs)
    {}
};
