#pragma once

#include "root.hpp"
#include "executable/Block.hpp"
#include "executable/VmFunc.hpp"
#include "executable/FunctionCollection.hpp"
#include "executable/StringTable.hpp"

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
