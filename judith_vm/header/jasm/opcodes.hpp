#pragma once

#include "root.hpp"

namespace OpCode {
    enum {
        NOOP = 0,

        NATIVE,

        CONST,
        CONST_L,
        CONST_L_L,
        CONST_0,
        F_CONST_1,
        F_CONST_2,
        I_CONST_1,
        I_CONST_2,
        STR_CONST,
        STR_CONST_L,

        RET,

        F_NEG,
        F_ADD,
        F_SUB,
        F_MUL,
        F_DIV,
        F_GT,
        F_GE,
        F_LT,
        F_LE,

        I_NEG,
        I_ADD,
        I_ADD_CHECKED,
        I_SUB,
        I_SUB_CHECKED,
        I_MUL,
        I_MUL_CHECKED,
        I_DIV,
        I_DIV_CHECKED,
        I_GT,
        I_GE,
        I_LT,
        I_LE,

        EQ,
        NEQ,

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

        JMP,
        JMP_L,
        JTRUE,
        JTRUE_L,
        JTRUE_K,
        JTRUE_K_L,
        JFALSE,
        JFALSE_L,
        JFALSE_K,
        JFALSE_K_L,

        CALL,

        PRINT,
    };
}