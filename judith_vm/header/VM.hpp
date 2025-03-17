#pragma once

#include "root.hpp"
#include "runtime/Assembly.hpp"
#include "runtime/Block.hpp"
#include "runtime/ConstantType.hpp"
#include "runtime/Value.hpp"
#include "runtime/object/Object.hpp"
#include "runtime/object/StringObject.hpp"
#include <ankerl/unordered_dense.h>
#include <stack>
#include "runtime/ExecutionContext.hpp"
#include <filesystem>
#include "data/AssemblyFile.hpp"
#include "runtime/InternedStringTable.hpp"
#include "runtime/NativeAssembly.hpp"

namespace fs = std::filesystem;

class JasmFunction;

#define STACK_MAX 1024
#define LOCALS_MAX 256 
#define LOCALS_EXT_MAX (USHRT_MAX + 1)

enum class InterpretResult {
    OK,
    RUNTIME_ERROR,
};

class VM {
private:
    InternedStringTable internedStrings;
    ExecutionContext execCtx;

    /// <summary>
    /// Maps every assembly that has been read to its file.
    /// </summary>
    ankerl::unordered_dense::map<std::string, AssemblyFile> assemblyFiles;

    /// <summary>
    /// Maps every assembly that has been loaded into its assembly object.
    /// </summary>
    ankerl::unordered_dense::map<std::string, Assembly&> assemblies;

    /// <summary>
    /// Contains all assemblies loaded (fully or partially) by this VM. The
    /// Assembly objects in the 'assemblies' map are just references to
    /// assemblies in this array.
    /// </summary>
    std::vector<u_ptr<Assembly>> loadedAssemblies;

    /// <summary>
    /// The assembly that was used to execute this. This assembly should be
    /// registered in the assemblies map.
    /// </summary>
    Assembly* executable = nullptr;

    /// <summary>
    /// Contains all the native types and functions.
    /// </summary>
    u_ptr<NativeAssembly> nativeAssembly;

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

public:
    VM(fs::path execPath);
    ~VM();

    /// <summary>
    /// Loads the given file as an executable assembly and starts execution by
    /// its entry point.
    /// </summary>
    /// <param name="entryPoint">A path to the executable, relative to the root
    /// path of the application.</param>
    void start(fs::path entryPoint);

    void execute (const JasmFunction& func);

private:
    void loadFullAssembly(const std::string& name, const AssemblyFile& file);
    //fs::path locateAssembly(std::string name);

public:
#pragma region Getters
    inline const Assembly& getExecutable () const {
        return *executable;
    }

    inline const AssemblyFile& getAssemblyFile (const std::string& name) const {
        return assemblyFiles.at(name);
    }

    inline const NativeAssembly& getNativeAssembly () const {
        return *nativeAssembly;
    }

    inline InternedStringTable& getInternedStringTable () {
        return internedStrings;
    }
#pragma endregion

    inline void resetStack () {
        stackTop = stack;
    }

    /// <summary>
    /// Places the given value at the top of the stack.
    /// </summary>
    /// <param name="value">The value to push.</param>
    inline void pushValue (Value value) {
#ifdef DEBUG_CHECK_OPERAND_STACK_BOUNDARIES
        if (stackTop == &stack[STACK_MAX - 1]) {
            throw std::runtime_error("Operand stack overflow!");
        }
#endif

        *stackTop = value;
        stackTop++;
    }

    /// <summary>
    /// Returns the value at the top of the stack, removing it from the stack.
    /// </summary>
    inline Value& popValue () {
#ifdef DEBUG_CHECK_OPERAND_STACK_BOUNDARIES
        if (stackTop == stack) {
            throw std::runtime_error("Trying to access an invalid stack position!");
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
        case ConstantType::STRING_UTF8:
            std::cout << value.asStringPtr->string; //.write(value.asStringPtr->string.get(), value.asStringPtr->length);
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