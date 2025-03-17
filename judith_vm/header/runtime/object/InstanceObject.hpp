#pragma once

#include "root.hpp"
#include "runtime/object/Object.hpp"
#include "runtime/Value.hpp"

struct Type;

struct InstanceObject {
    Object object;
    Type* type;
    void* fieldTable;
};

struct BoxObject {
    Object object;
    Type* type;
    Value value;
};