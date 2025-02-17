#pragma once
#include "root.hpp"

namespace ObjectType {
    enum : byte {
        STRING, // UTF-8
        FUNCTION,
        INSTANCE,
    };
}

struct Object {
    byte objectType;
    // gcHandle
};

struct StringObject {
    Object object;
    size_t length;
    u_ptr<char[]> string;

    StringObject (size_t length, const std::string& str)
        : object({ .objectType = ObjectType::STRING }),
        length(length)
    {
        //string = (u_ptr<char>)(char*)std::malloc(sizeof(char) * length);
        string = make_u<char[]>(length);
        std::memcpy(this->string.get(), str.c_str(), length);
    }
};

struct InstanceObject {
    Object object;
    void* fieldTable;
};