#pragma once

#include "root.hpp"

union Value {
    f64 asFloat64;
    i64 asInt64;
    bool asBool;
};
