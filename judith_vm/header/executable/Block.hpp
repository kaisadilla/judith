#pragma once

#include "root.hpp"
#include "executable/Function.hpp"

struct Block {
public:
    /// <summary>
    /// A pointer to the start of the raw string table.
    /// </summary>
    u_ptr<byte[]> stringTable;
    /// <summary>
    /// The size (amount of bytes) in the string table.
    /// </summary>
    size_t stringCount;
    /// <summary>
    /// An array where each element is a pointer to the constant at that index
    /// in the table.
    /// </summary>
    u_ptr<byte*[]> strings; // pointers here are owned by stringTable.
    /// <summary>
    /// An array with the functions that exist in this block.
    /// </summary>
    OWNED Function* functions; // TODO: Make into a unique ptr
    /// <summary>
    /// The amount of functions in the functions array.
    /// </summary>
    size_t functionCount;

public:
    Block(
        u_ptr<byte[]> stringTable,
        size_t stringCount,
        u_ptr<byte*[]> strings,
        Function* functions,
        size_t functionCount
    );

    Block(const Block&) = delete;
    Block& operator=(const Block&) = delete;
    Block(Block&&) noexcept = default;
    Block& operator=(Block&&) noexcept = default;

    ~Block();
};