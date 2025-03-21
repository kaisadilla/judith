#pragma once
#include "root.hpp"

namespace ObjectType {
    enum : byte {
        INVALID,
        UTF8_STRING, // A value of the type String.
        FUNCTION, // A value containing a function.
        INSTANCE, // A value containing the instance of an object.
        BOX, // A value containing a boxed vm Value.
    };
}

struct Object {
    byte objectType;
    // gcHandle
};
