#pragma once
#include "root.hpp"

namespace ObjectType {
    enum : byte {
        UTF8_STRING, // UTF-8
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
        : object({ .objectType = ObjectType::UTF8_STRING }),
        length(length)
    {
        //string = (u_ptr<char>)(char*)std::malloc(sizeof(char) * length);
        string = make_u<char[]>(length);
        std::memcpy(this->string.get(), str.c_str(), length);
    }
};

struct FunctionObject {
    Object object;
    std::string name;
    u_ptr<Chunk> chunk; // TODO: Chunk.

    FunctionObject (std::string name, u_ptr<Chunk> chunk)
        : name(std::move(name)), chunk(std::move(chunk))
    {}
};

struct InstanceObject {
    Object object;
    void* fieldTable;
};