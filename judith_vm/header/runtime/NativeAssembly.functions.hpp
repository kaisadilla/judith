#pragma once

#include "root.hpp"

class VM;

namespace NativeFunctions {
    void errorFunc (VM& vm);
    void print (VM& vm);
    void println (VM& vm);
    void readln (VM& vm);
}