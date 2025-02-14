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
    char* string;
};

struct InstanceObject {
    Object object;
    void* fieldTable;
};