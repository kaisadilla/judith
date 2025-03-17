#pragma once
#include "root.hpp"

namespace ObjectType {
    enum : byte {
        INVALID,
        UTF8_STRING, // UTF-8
        FUNCTION,
        INSTANCE,
    };
}

struct Object {
    byte objectType;
    // gcHandle
};
