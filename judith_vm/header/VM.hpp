#pragma once

#include "root.hpp"
#include "Chunk.hpp"
#include "Value.hpp"

#define STACK_MAX 1024

enum class InterpretResult {
    OK,
    RUNTIME_ERROR,
};

class VM {
private:
    /// <summary>
    /// This VM's execution stack.
    /// </summary>
    Value stack[STACK_MAX];
    /// <summary>
    /// A pointer to the first unused value of the stack.
    /// </summary>
    Value* stackTop = stack;

public:
    InterpretResult interpret (const Chunk& chunk);

    inline void resetStack () {
        stackTop = stack;
    }

    /// <summary>
    /// Places the given value at the top of the stack.
    /// </summary>
    /// <param name="value">The value to push.</param>
    inline void pushValue (Value value) {
        *stackTop = value;
        stackTop++;
    }

    /// <summary>
    /// Returns the value at the top of the stack, removing it from the stack.
    /// </summary>
    inline Value& popValue () {
        stackTop--;
        return *stackTop;
    }
    
    inline void printValue (const Value& value) {
        std::cout << value.asFloat64;
    }
};