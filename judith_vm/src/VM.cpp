#include "VM.hpp"
#include "jasm/opcodes.hpp"
#include "runtime/VmFunc.hpp"
#include "loader/AssemblyLoader.hpp"

#pragma region Macros

#define READ_BYTE() (*(ip++))
#define READ_SBYTE() ((sbyte)*(ip++))
#define READ_U16() (ip += 2, *(ip - 2) | (*(ip - 1) << 8))
#define READ_I32() (ip += 4, *(ip - 4) | (*(ip - 3) << 8) | (*(ip - 2) << 16) | (*(ip - 1) << 24))
#define READ_U32() (ip += 4, *(ip - 4) | (*(ip - 3) << 8) | (*(ip - 2) << 16) | (*(ip - 1) << 24))
#define READ_I64() (ip += 8, *(i64*)(ip - 8))

#define F_BINARY_OP(op) \
    do { \
        f64 b = popValue().asFloat64; \
        f64 a = popValue().asFloat64; \
        pushValue({ .asFloat64 = (a op b) }); \
    } while (false)

#define F_BOOLEAN_OP(op) \
    do { \
        f64 b = popValue().asFloat64; \
        f64 a = popValue().asFloat64; \
        pushValue({ .asInt64 = (a op b) }); \
    } while (false)

#define I_BOOLEAN_OP(op) \
    do { \
        f64 b = popValue().asInt64; \
        f64 a = popValue().asInt64; \
        pushValue({ .asInt64 = (a op b) }); \
    } while (false)

#pragma endregion

VM::VM (fs::path execPath) : execCtx(execPath)
{

}

VM::~VM () {

}

void VM::start (fs::path entryPoint) {
    fs::path filePath = execCtx.appDirectory / entryPoint;
    const std::string& assemblyName = entryPoint.stem().string();

    assemblyFiles.emplace(assemblyName, AssemblyFile::loadFromFile(filePath.string().c_str()));

    //assemblies.emplace(assemblyName, readAssembly(this, filePath.string().c_str()));
    //executable = &assemblies.at(assemblyName);
}

void VM::interpret (const Assembly& assembly) {
    this->assembly = &assembly;
    //execute(assembly.blocks[0].functions[0]); // TODO PRT: Reenable
}

void VM::execute (const VmFunc& func) {
#ifdef DEBUG_PRINT_CALL_STACK
    std::cout << "\n===== ENTERING FUNC " << &func << " ===== \n";
#endif
    enterFunction(func.maxLocals);

    const Chunk& chunk = func.chunk;
    byte* ip = chunk.code.get();
    
    while (true) {
#ifdef DEBUG_PRINT_STACK
        std::cout << "stack: [";
        for (Value* slot = stack; slot < stackTop; slot++) {
            std::cout << slot->asFloat64 << ", ";
        }
        std::cout << "]" << std::endl;
#endif
#ifdef DEBUG_CHECK_STACK_UNDERFLOW
        if (ip < chunk.code.get()) {
            throw std::exception("Trying to access an invalid stack position!");
        }
#endif

        byte instruction = *(ip++);

        switch (instruction) {
        case OpCode::NOOP:
            break;

        case OpCode::NATIVE:
            break;

        case OpCode::CONST:
            pushValue({ .asInt64 = *(i64*)READ_BYTE() });

            break;

        case OpCode::CONST_L:
            pushValue({ .asInt64 = *(i64*)READ_I32() });

            break;

        case OpCode::CONST_L_L:
            pushValue({ .asInt64 = READ_I64() });

            break;

        case OpCode::CONST_0:
            pushValue({ .asInt64 = 0});

            break;

        case OpCode::F_CONST_1:
            pushValue({ .asFloat64 = 1});

            break;

        case OpCode::F_CONST_2:
            pushValue({ .asFloat64 = 2});

            break;

        case OpCode::I_CONST_1:
            pushValue({ .asInt64 = 1});

            break;

        case OpCode::I_CONST_2:
            pushValue({ .asInt64 = 2});

            break;

        case OpCode::STR_CONST: {
            #define STRLEN (*(ui64*)strval)
            #define STRHEAD ((char*)(strval + sizeof(ui64)))

            byte* strval = (byte*)chunk.strings[READ_BYTE()];
            std::string str(STRHEAD, STRLEN);
            pushValue({ .asStringPtr = internString(str) });
            break;

            #undef STRLEN
            #undef STRHEAD
        }
            break;

        case OpCode::STR_CONST_L:
            pushValue({ .asInt64 = *(i64*)chunk.strings[READ_I32()] });

            break;

        case OpCode::RET:
#ifdef DEBUG_PRINT_CALL_STACK
            std::cout << "===== EXITING FUNC " << &func << " =====\n\n";
#endif
            exitFunction();
            return;

        case OpCode::F_NEG:
            pushValue({ .asFloat64 = -popValue().asFloat64 });

            break;

        case OpCode::F_ADD:
            F_BINARY_OP(+);

            break;

        case OpCode::F_SUB:
            F_BINARY_OP(-);

            break;

        case OpCode::F_MUL:
            F_BINARY_OP(*);

            break;

        case OpCode::F_DIV:
            F_BINARY_OP(/);

            break;

        case OpCode::F_GT:
            F_BOOLEAN_OP(>);

            break;

        case OpCode::F_GE:
            F_BOOLEAN_OP(>=);

            break;

        case OpCode::F_LT:
            F_BOOLEAN_OP(<);

            break;

        case OpCode::F_LE:
            F_BOOLEAN_OP(<=);

            break;

        case OpCode::I_ADD:
            break;

        case OpCode::I_SUB:
            break;

        case OpCode::I_MUL:
            break;

        case OpCode::I_DIV:
            break;

        case OpCode::I_GT:
            break;

        case OpCode::I_GE:
            break;

        case OpCode::I_LT:
            break;

        case OpCode::I_LE:
            break;

        case OpCode::EQ:
            I_BOOLEAN_OP(==);
            break;

        case OpCode::NEQ:
            I_BOOLEAN_OP(!=);
            break;

        case OpCode::STORE_0:
            storeLocal(0);
            break;

        case OpCode::STORE_1:
            storeLocal(1);
            break;

        case OpCode::STORE_2:
            storeLocal(2);
            break;

        case OpCode::STORE_3:
            storeLocal(3);
            break;

        case OpCode::STORE_4:
            storeLocal(4);
            break;

        case OpCode::STORE:
            storeLocal(READ_BYTE());
            break;

        case OpCode::STORE_L:
            throw "Extended local variable array not implemented.";
            //storeLocal(READ_U16());
            break;

        case OpCode::LOAD_0:
            loadLocal(0);
            break;

        case OpCode::LOAD_1:
            loadLocal(1);
            break;

        case OpCode::LOAD_2:
            loadLocal(2);
            break;

        case OpCode::LOAD_3:
            loadLocal(3);
            break;

        case OpCode::LOAD_4:
            loadLocal(4);
            break;

        case OpCode::LOAD:
            loadLocal(READ_BYTE());
            break;

        case OpCode::LOAD_L:
            throw "Extended local variable array not implemented.";
            //loadLocal(READ_U16());
            break;

        case OpCode::JMP: {
            ip += READ_SBYTE();
            break;
        }

        case OpCode::JTRUE: {
            sbyte offset = READ_SBYTE();
            if (popValue().asInt64) {
                ip += offset;
            }
            break;
        }

        case OpCode::JTRUE_K: {
            sbyte offset = READ_SBYTE();
            if (peekValue().asInt64) {
                ip += offset;
            }
            else {
                popValue();
            }
            break;
        }

        case OpCode::JFALSE: {
            sbyte offset = READ_SBYTE();
            if (popValue().asInt64 == 0) {
                ip += offset;
            }

            break;
        }

        case OpCode::JFALSE_K: {
            sbyte offset = READ_SBYTE();
            if (peekValue().asInt64 == 0) {
                ip += offset;
            }
            else {
                popValue();
            }

            break;
        }

        case OpCode::CALL: {
            ui32 index = READ_U32();
            // execute(*(assembly->assemblyFunctions[index])); // TODO PRT: Reenable
            break;
        }

        case OpCode::PRINT:
            printValue(READ_BYTE(), popValue());
            std::cout << "\n";
            break;

        default:
            std::cerr << "[ERROR] UNKNOWN OPCODE: " << std::hex << (int)instruction
                << std::dec << std::endl;
            break;
        }
    }

    exitFunction();
}

void VM::loadFullAssembly(const std::string& name, const AssemblyFile& file) {

}

#undef READ_BYTE
#undef READ_I32
#undef F_BINARY_OP