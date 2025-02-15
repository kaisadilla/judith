#pragma once

#include "root.hpp"

namespace OpCode {
    enum {
        NOOP = 0,

        CONST,
        CONST_LONG,
        CONST_0,
        I_CONST_1,
        I_CONST_2,
        CONST_STR,
        CONST_STR_LONG,

        RET,

        F_NEG,
        F_ADD,
        F_SUB,
        F_MUL,
        F_DIV,
        I_NEG,

        I_ADD,
        I_ADD_CHECKED,
        I_SUB,
        I_SUB_CHECKED,
        I_MUL,
        I_MUL_CHECKED,
        I_DIV,
        I_DIV_CHECKED,

        STORE_0,
        STORE_1,
        STORE_2,
        STORE_3,
        STORE_4,
        STORE,
        STORE_L,

        LOAD_0,
        LOAD_1,
        LOAD_2,
        LOAD_3,
        LOAD_4,
        LOAD,
        LOAD_L,

        PRINT,
    };
}