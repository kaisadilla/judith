#include "executable/FuncRef.hpp"
#include "executable/FunctionCollection.hpp"
#include "VM.hpp"

void InternalFuncRef::loadFunction(VM& vm) {
    funcCollection->setFunction(index, &FuncRef::callFunction);
}

void InternalFuncRef::callFunction(VM& vm) {
#ifdef DEBUG_CHECK_FUNC_REF_LOADED
    if (vmFunc == nullptr) {
        throw "Trying to call a function reference that hasn't been loaded.";
    }
#endif

    vm.execute(*vmFunc);
}

void NativeFuncRef::loadFunction(VM& vm) {
    funcCollection->setFunction(index, &FuncRef::callFunction);
}

void NativeFuncRef::callFunction(VM& vm) {
#ifdef DEBUG_CHECK_FUNC_REF_LOADED
    if (vmFunc == nullptr) {
        throw "Trying to call a function reference that hasn't been loaded.";
    }
#endif

    vm.execute(*vmFunc);
}
