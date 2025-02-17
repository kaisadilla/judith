#pragma once

#include "root.hpp"

struct Object;
struct StringObject;

union Value {
    f64 asFloat64;
    i64 asInt64;
    ui64 asUint64;
    bool asBool;
    Object* asObjectPtr;
    StringObject* asStringPtr;
};
