#pragma once

#include "root.hpp"

struct StringObject;

namespace TypeKind {
    enum : byte {
        ERROR,
        PSEUDO,
        PRIMITIVE,
        STRING_UTF8,
        FUNCTION,
        ARRAY,
        STRUCT,
        DYNAMIC,
        NULL_TYPE,
        UNDEFINED,
    };
}

struct Type {
    byte kind;
    StringObject* name;
};

struct PrimitiveType {
    Type type;

    PrimitiveType (StringObject* name) : type({
        .kind = TypeKind::PRIMITIVE,
        .name = name
    }) {}
};

struct StringType {
    Type type;

    StringType (StringObject* name) : type({
        .kind = TypeKind::STRING_UTF8,
        .name = name,
    }) {}
};