#pragma once

#include "root.hpp"
#include "runtime/object/Object.hpp"

struct StringObject {
    Object object;
    size_t length;
    char* string;

    StringObject (size_t length, const std::string& str)
        : object({ .objectType = ObjectType::UTF8_STRING }),
        length(length)
    {
        string = new char[length + 1];
        memcpy(string, str.c_str(), length);
        string[length] = '\0';
    }

    /// <summary>
    /// Creates a string object from a string located inside a StringTable.
    /// The pointer must point to the first byte of the string, which represents
    /// the size of the string.
    /// </summary>
    /// <param name="binaryString">A pointer inside a StringTable where the
    /// string's size begins.</param>
    StringObject (const byte* binaryString)
        : object({ .objectType = ObjectType::UTF8_STRING })
    {
        memcpy(&length, binaryString, sizeof(size_t));

        string = new char[length + 1];
        memcpy(string, binaryString + sizeof(size_t), length);
        string[length] = '\0';
    }

    ~StringObject () {
        delete[] string;
    }
};
