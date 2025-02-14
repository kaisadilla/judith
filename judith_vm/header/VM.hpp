#pragma once

#include "root.hpp"
#include "Chunk.hpp"
#include "Value.hpp"

#define STACK_MAX 1024
#define LOCALS_MAX 256 
#define LOCALS_EXT_MAX (USHRT_MAX + 1)

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

    /// <summary>
    /// The VM's local variable array.
    /// </summary>
    Value locals[LOCALS_MAX];

public:
    ~VM();

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

    /// <summary>
    /// Pops the value at the top of the stack and stores it in the local given.
    /// </summary>
    /// <param name="index"></param>
    inline void storeLocal (size_t index) {
        locals[index] = popValue();
    }

    /// <summary>
    /// Pushes the value at the local given to the stack.
    /// </summary>
    /// <param name="index"></param>
    inline void loadLocal (size_t index) {
        pushValue(locals[index]);
    }
    
    inline void printValue (const Value& value) {
        std::cout << value.asFloat64;
    }
};