#pragma once

#include "root.hpp"
#include "ankerl/unordered_dense.h"
#include "runtime/object/StringObject.hpp"

class InternedStringTable {
    ankerl::unordered_dense::map<std::string, u_ptr<StringObject>> stringTable;

public:
    inline StringObject* getStringObject (const std::string& str) {
        auto [it, inserted] = stringTable.try_emplace(
            str, make_u<StringObject>(str.length(), str)
        );

        return it->second.get();
    }

    /// <summary>
    /// Given a pointer to a byte inside a StringTable, reads the string starting
    /// at that pointer, interns it if needed, and returns the interned
    /// StringObject that contains said string.
    /// </summary>
    /// <param name="stringTableOffset">A pointer to a byte inside a StringTable.</param>
    inline StringObject* getStringObject (byte* stringTableOffset) {
        const size_t len = *reinterpret_cast<const size_t*>(stringTableOffset);

        const std::string str(
            reinterpret_cast<const char*>(stringTableOffset + sizeof(size_t)),
            len
        );

        return getStringObject(str);
    }
};