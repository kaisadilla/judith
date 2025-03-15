#pragma once

#include <root.hpp>
#include <functional>
#include "runtime/FuncRef.hpp"

class VM;

using FunctionPtr = void(FuncRef::*)(VM&);

class FunctionCollection {
    friend class Assembly; // FunctionCollection is always built by Assembly.

private:
    OWNED OWNED FuncRef** references;

    FunctionPtr* functions;
    size_t count;

public:
    FunctionCollection (size_t count)
        : references(new FuncRef*[count]),
        functions(new FunctionPtr[count]),
        count(count)
    {}

    ~FunctionCollection () {
        for (size_t fr = 0; fr < count; fr++) {
            delete references[fr];
        }

        delete[] references;
    }

    inline void callFunction (size_t index, VM& vm) {
        ((*references[index]).*functions[index])(vm);
    }

    inline void setFunction (size_t index, FunctionPtr funcPtr) {
        functions[index] = funcPtr;
    }
};