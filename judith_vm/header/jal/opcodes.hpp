#pragma once

#include "root.hpp"

namespace OpCode {
    enum {
        NOOP = 0,
        CONSTANT,
        CONSTANT_LONG,
        RETURN,
        NEGATE,
        ADD,
        SUBTRACT,
        MULTIPLY,
        DIVIDE,
        CHECKED_ADD,
        CHECKED_SUBTRACT,
        CHECKED_MULTIPLY,
        CHECKED_DIVIDE,

        PRINT,
    };
}