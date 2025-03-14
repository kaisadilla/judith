#pragma once

#include "root.hpp"

struct StringTable {
    /// <summary>
    /// A pointer to the start of the table.
    /// </summary>
    u_ptr<byte[]> table;

    /// <summary>
    /// The size, in bytes, of the table.
    /// </summary>
    size_t size;

    /// <summary>
    /// An array where each index contains the address of a string inside the
    /// table.
    /// </summary>
    u_ptr<byte*[]> strings;

    /// <summary>
    /// The amount of strings in the table.
    /// </summary>
    size_t count;

    StringTable (u_ptr<byte[]> table, size_t size, u_ptr<byte*[]> strings, size_t count)
        : table(std::move(table)),
        size(size),
        strings(std::move(strings)),
        count(count)
    {}

    StringTable (const StringTable&) = delete;
    StringTable& operator= (const StringTable&) = delete;
    StringTable(StringTable&&) noexcept = default;
    StringTable& operator= (StringTable&&) noexcept = default;

    inline std::string getString (int index) const {
        size_t size = *strings[index];

        return std::string(reinterpret_cast<char*>(strings[index] + sizeof(size_t)), size);
    }
};