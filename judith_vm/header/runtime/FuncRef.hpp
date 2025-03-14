#pragma once

#include "root.hpp"

class VM;
struct VmFunc;
class FunctionCollection;

class FuncRef {
public:
    virtual void loadFunction (VM& vm) abstract;
    virtual void callFunction (VM& vm) abstract;
};

class InternalFuncRef : public FuncRef {
private:
    FunctionCollection* funcCollection;
    size_t index;

    VmFunc* vmFunc;

public:
    void loadFunction (VM& vm) override;
    void callFunction (VM& vm) override;
};

class NativeFuncRef : public FuncRef {
private:
    FunctionCollection* funcCollection;
    size_t index;

    VmFunc* vmFunc;

public:
    void loadFunction (VM& vm) override;
    void callFunction (VM& vm) override;
};