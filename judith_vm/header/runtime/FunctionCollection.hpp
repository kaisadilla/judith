#pragma once

#include <root.hpp>
#include <functional>
#include "runtime/FuncRef.hpp"

class VM;

using FunctionPtr = void(FuncRef::*)(VM&);

class FunctionCollection {
private:
    OWNED FuncRef* references;
    FunctionPtr* functions;
    size_t count;

public:
    /// <summary>
    /// 
    /// </summary>
    /// <param name="references">An array of function references created with new[].</param>
    /// <param name="functions"></param>
    /// <param name="count"></param>
    FunctionCollection(FuncRef* references, FunctionPtr* functions, size_t count) :
        references(references),
        functions(functions),
        count(count)
    {}

    ~FunctionCollection () {
        delete[] references;
    }

    inline void callFunction (size_t index, VM& vm) {
        (references[index].*functions[index])(vm);
    }

    inline void setFunction (size_t index, FunctionPtr funcPtr) {
        functions[index] = funcPtr;
    }
};