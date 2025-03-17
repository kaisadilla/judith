#include "runtime/NativeAssembly.hpp"
#include "runtime/NativeAssembly.Functions.hpp"
#include "runtime/type/Type.hpp";
#include "VM.hpp";

NativeAssembly::NativeAssembly(VM& vm) {
    types[NATIVE_TYPE_INDEX_F64] = (Type*)new PrimitiveType(
        vm.getInternedStringTable().getStringObject(NativeTypeNames::F64)
    );

    types[NATIVE_TYPE_INDEX_BOOL] = (Type*)new PrimitiveType(
        vm.getInternedStringTable().getStringObject(NativeTypeNames::BOOL)
    );

    types[NATIVE_TYPE_INDEX_STRING] = (Type*)new StringType(
        vm.getInternedStringTable().getStringObject(NativeTypeNames::STRING)
    );

    functions[NATIVE_FUNC_ERROR] = NativeFunctions::errorFunc;
    functions[NATIVE_FUNC_PRINT] = NativeFunctions::print;
    functions[NATIVE_FUNC_PRINTLN] = NativeFunctions::println;
    functions[NATIVE_FUNC_READLN] = NativeFunctions::readln;
}
