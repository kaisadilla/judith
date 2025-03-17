#pragma once

#include "root.hpp"
#include "runtime/NativeAssembly.constants.hpp"

struct Type;
class VM;

using NativeFuncPtr = void(*)(VM&);

class NativeAssembly {
    OWNED Type* types[NativeTypeCount];

    OWNED NativeFuncPtr functions[NativeFuncCount];

public:
    NativeAssembly (VM& vm);

    inline NativeFuncPtr getFunc (size_t index) const {
        return functions[index];
    }
};