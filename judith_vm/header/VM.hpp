#pragma once

#include "root.hpp"
#include "executable/Assembly.hpp"
#include "executable/Block.hpp"
#include "executable/ConstantType.hpp"
#include "Value.hpp"
#include "Object.hpp"
#include <ankerl/unordered_dense.h>
#include <stack>

struct Function;

#define STACK_MAX 1024
#define LOCALS_MAX 256 
#define LOCALS_EXT_MAX (USHRT_MAX + 1)

enum class InterpretResult {
    OK,
    RUNTIME_ERROR,
};

class VM {
private:
    std::stack<Value*> localArrayStack;

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
    Value* locals = nullptr;

    /// <summary>
    /// The string table maps strings to internet string objects.
    /// </summary>
    ankerl::unordered_dense::map<std::string, u_ptr<StringObject>> stringTable;

    const Assembly* assembly = nullptr;

public:
    ~VM();

    void interpret (const Assembly& assembly);
    void execute (const Function& func);

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
#ifdef DEBUG_CHECK_STACK_UNDERFLOW
        if (stackTop == stack) {
            throw std::exception("Trying to access an invalid stack position!");
        }
#endif
        stackTop--;
        return *stackTop;
    }

    /// <summary>
    /// Returns the value at the top of the stack, but doesn't remove it.
    /// </summary>
    /// <returns></returns>
    inline Value& peekValue () {
#ifdef DEBUG_CHECK_IP_UNDERFLOW
        if (stackTop == stack) {
            throw std::exception("Trying to access an invalid stack position!");
        }
#endif
        return *(stackTop - 1);
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

    inline void enterFunction (size_t maxLocals) {
        localArrayStack.push(locals);
        locals = new Value[maxLocals];
    }

    inline void exitFunction () {
        delete[] locals;
        locals = localArrayStack.top();
        localArrayStack.pop();
    }

    inline StringObject* internString (std::string str) {
        auto [it, inserted] = stringTable.try_emplace(
            str, make_u<StringObject>(str.length(), str)
        );

        return it->second.get();
    }
    
    inline void printValue (byte type, const Value& value) {
        switch (type) {
        case ConstantType::INT_64:
            std::cout << value.asInt64;
            break;
        case ConstantType::FLOAT_64:
            std::cout << value.asFloat64;
            break;
        case ConstantType::UNSIGNED_INT_64:
            std::cout << value.asUint64;
            break;
        case ConstantType::STRING_ASCII:
            std::cout.write(value.asStringPtr->string.get(), value.asStringPtr->length);
            break;
        case ConstantType::BOOL:
            std::cout << value.asFloat64;
            //std::cout << (value.asInt64 == 0 ? "false" : "true");
            break;
        default:
            std::cout << "Error: unknown type.";
        }
    }
};

#undef STACK_MA
#undef LOCALS_MAX
#undef LOCALS_EXT_MAX