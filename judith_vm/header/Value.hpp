#pragma once

#include "root.hpp"

struct Object;

union Value {
    f64 asFloat64;
    i64 asInt64;
    bool asBool;
    Object* asObject;
};
