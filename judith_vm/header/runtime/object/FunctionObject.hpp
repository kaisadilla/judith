#pragma once

#include "root.hpp"
#include "runtime/object/Object.hpp"

class JasmFunction;

struct FunctionObject {
    Object object;
    JasmFunction* function;
};