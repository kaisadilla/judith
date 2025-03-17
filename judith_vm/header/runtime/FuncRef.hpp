#pragma once

#include "root.hpp"

class VM;
class JasmFunction;
class Assembly;

using NativeFuncPtr = void(*)(VM&);

class FuncRef {
public:
    virtual void loadFunction (VM& vm) abstract;
    virtual void callFunction (VM& vm) abstract;
};

class InternalFuncRef : public FuncRef {
private:
    Assembly& assembly;
    size_t index;

    JasmFunction* func;

public:
    InternalFuncRef(Assembly& assembly, size_t index, JasmFunction* func)
        : assembly(assembly),
        index(index),
        func(func)
    {}

    void loadFunction (VM& vm) override;
    void callFunction (VM& vm) override;
};

class NativeFuncRef : public FuncRef {
private:
    Assembly& assembly;
    size_t index;

    NativeFuncPtr nativeFunc;

public:
    NativeFuncRef(Assembly& assembly, size_t index, NativeFuncPtr nativeFunc)
        : assembly(assembly),
        index(index),
        nativeFunc(nativeFunc)
    {}

    void loadFunction (VM& vm) override;
    void callFunction (VM& vm) override;
};