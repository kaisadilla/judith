#pragma once

#include "root.h"
#include "runtime/object/Object.hpp"

class JasmFunction;

struct FunctionObject {
    Object object;
    JasmFunction* function;
};