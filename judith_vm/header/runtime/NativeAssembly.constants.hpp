#pragma once

#include "root.hpp"

enum NativeTypeIndex : byte {
    NATIVE_TYPE_INDEX_ERROR = 0,
    NATIVE_TYPE_INDEX_I8,
    NATIVE_TYPE_INDEX_I16,
    NATIVE_TYPE_INDEX_I32,
    NATIVE_TYPE_INDEX_I64,
    NATIVE_TYPE_INDEX_UI8,
    NATIVE_TYPE_INDEX_UI16,
    NATIVE_TYPE_INDEX_UI32,
    NATIVE_TYPE_INDEX_UI64,
    NATIVE_TYPE_INDEX_F32,
    NATIVE_TYPE_INDEX_F64,
    NATIVE_TYPE_INDEX_BIGINT,
    NATIVE_TYPE_INDEX_DECIMAL,
    NATIVE_TYPE_INDEX_BOOL,
    NATIVE_TYPE_INDEX_STRING,
    NATIVE_TYPE_INDEX_REGEX,
    _NATIVE_TYPE_COUNT,
};

constexpr int NativeTypeCount = _NATIVE_TYPE_COUNT;

namespace NativeTypeNames {
    constexpr const char* F64 = "F64";
    constexpr const char* BOOL = "Bool";
    constexpr const char* STRING = "String";
}

enum NativeFuncIndex {
    NATIVE_FUNC_ERROR = 0,
    NATIVE_FUNC_PRINT,
    NATIVE_FUNC_PRINTLN,
    NATIVE_FUNC_READLN,
    _NATIVE_FUNC_COUNT,
};

constexpr int NativeFuncCount = _NATIVE_FUNC_COUNT;

namespace NativeFuncNames {
    constexpr const char* PRINT = "print";
}