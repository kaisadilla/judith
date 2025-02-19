#pragma once

#include "root.hpp"
#include "executable/Function.hpp"

struct Block {
public:
    /// <summary>
    /// A pointer to the start of the raw constant table.
    /// </summary>
    u_ptr<byte[]> constantTable;
    /// <summary>
    /// The size (amount of bytes) in the constant table.
    /// </summary>
    size_t constantCount;
    /// <summary>
    /// An array where each element is a pointer to the constant at that index
    /// in the table.
    /// </summary>
    u_ptr<void*[]> constants; // pointers here are owned by constantTable.
    /// <summary>
    /// An array where each element is a byte representing the constant type
    /// at that index in the table.
    /// </summary>
    u_ptr<byte[]> constantTypes;
    /// <summary>
    /// An array with the functions that exist in this block.
    /// </summary>
    /*OWNED*/ Function* functions; // TODO: Make into a unique ptr
    /// <summary>
    /// The amount of functions in the functions array.
    /// </summary>
    size_t functionCount;

public:
    Block(
        u_ptr<byte[]> constantTable,
        size_t constantCount,
        u_ptr<void*[]> constants,
        u_ptr<byte[]> constantTypes,
        Function* functions,
        size_t functionCount
    );

    ~Block();
};