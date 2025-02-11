#pragma once

#include "root.hpp"

enum class ValueType {
    FLOAT64
};

struct Value {
    ValueType type;
    union {
        f64 floatNum;
    } as;
};
