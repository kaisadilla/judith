#include "runtime/FuncRef.hpp"
#include "runtime/FunctionCollection.hpp"
#include "VM.hpp"

void InternalFuncRef::loadFunction(VM& vm) {
    if (func == nullptr) {
        throw "Loading functions not yet supported.";
    }

    assembly.functionRefs.setFunction(index, &FuncRef::callFunction);

    callFunction(vm);
}

void InternalFuncRef::callFunction(VM& vm) {
#ifdef DEBUG_CHECK_FUNC_REF_LOADED
    if (func == nullptr) {
        throw "Trying to call a function reference that hasn't been loaded.";
    }
#endif

    vm.execute(*func);
}

void NativeFuncRef::loadFunction(VM& vm) {
    assembly.functionRefs.setFunction(index, &FuncRef::callFunction);
}

void NativeFuncRef::callFunction(VM& vm) {
#ifdef DEBUG_CHECK_FUNC_REF_LOADED
    if (vmFunc == nullptr) {
        throw "Trying to call a function reference that hasn't been loaded.";
    }
#endif

    vm.execute(*vmFunc);
}
