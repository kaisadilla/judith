#pragma once

#include "root.hpp"

enum class ValueType {
    FLOAT64
};

struct Value {
    ValueType type;
    union {
        double floatNum;
    } as;
};
