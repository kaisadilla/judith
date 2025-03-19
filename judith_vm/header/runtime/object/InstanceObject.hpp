#pragma once

#include "root.hpp"
#include "runtime/object/Object.hpp"
#include "runtime/Value.hpp"

struct Type;

struct InstanceObject {
    Object object;
    Type* type;
    /// <summary>
    /// The first byte in the field table. Notice that instance objects cannot
    /// be created directly, and 
    /// </summary>
    byte fieldTable;

    InstanceObject () {}
};

struct BoxObject {
    Object object;
    Type* type;
    Value value;
};

InstanceObject* makeInstance () {
    throw "Not implemented";
}

void freeInstance (InstanceObject* instance) {
    throw "Not implemented";
}