#include "VM.hpp"
#include "jal/opcodes.hpp"

#define READ_BYTE() (*(ip++))
#define READ_U16() (*(ip++) | (*(ip++) << 8)) // TODO: Check that unsigned usage is implemented properly.
#define READ_I32() (*(ip++) | (*(ip++) << 8) | (*(ip++) << 16) | (*(ip++) << 24))

#define BINARY_OP(op) \
    do { \
        f64 b = popValue().asFloat64; \
        f64 a = popValue().asFloat64; \
        pushValue({ .asFloat64 = (a op b) }); \
    } while (false)

VM::~VM() {

}

InterpretResult VM::interpret (const Chunk& chunk) {
    byte* ip = chunk.code;

    while (true) {
#ifdef DEBUG_DUMP_STACK
        std::cout << "stack: [";
        for (Value* slot = stack; slot < stackTop; slot++) {
            std::cout << slot->asFloat64 << ", ";
        }
        std::cout << "]" << std::endl;
#endif

        byte instruction = *(ip++);
        switch (instruction) {
        case OpCode::NOOP:
            break;

        case OpCode::CONST:
            pushValue({ .asInt64 = *(i64*)chunk.constants[READ_BYTE()] });

            break;

        case OpCode::CONST_LONG:
            pushValue({ .asInt64 = *(i64*)chunk.constants[READ_I32()] });

            break;

        case OpCode::CONST_0:
            pushValue({ .asInt64 = 0});

            break;

        case OpCode::I_CONST_1:
            pushValue({ .asInt64 = 1});

            break;

        case OpCode::I_CONST_2:
            pushValue({ .asInt64 = 2});

            break;

        case OpCode::CONST_STR: {
#define STRLEN (*(ui64*)strval)
#define STRHEAD ((char*)(strval + sizeof(ui64)))

            byte* strval = (byte*)chunk.constants[READ_BYTE()];
            std::string str(STRHEAD, STRLEN);
            pushValue({ .asStringPtr = internString(str) });
            break;

#undef STRLEN
#undef STRHEAD
        }
            break;

        case OpCode::CONST_STR_LONG:
            pushValue({ .asInt64 = *(i64*)chunk.constants[READ_I32()] });

            break;

        case OpCode::RET:
            return InterpretResult::OK;

        case OpCode::F_NEG:
            pushValue({ .asFloat64 = -popValue().asFloat64 });

            break;

        case OpCode::F_ADD:
            BINARY_OP(+);

            break;

        case OpCode::F_SUB:
            BINARY_OP(-);

            break;

        case OpCode::F_MUL:
            BINARY_OP(*);

            break;

        case OpCode::F_DIV:
            BINARY_OP(/);

            break;

        case OpCode::I_ADD:
            break;

        case OpCode::I_SUB:
            break;

        case OpCode::I_MUL:
            break;

        case OpCode::I_DIV:
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

        case OpCode::PRINT:
            printValue(READ_BYTE(), popValue());
            std::cout << "\n";
            break;

        default:
            std::cerr << "[ERROR] UNKNOWN OPCODE: " << std::hex << instruction
                << std::dec << std::endl;
            break;
        }
    }

    return InterpretResult::OK;
}

#undef READ_BYTE
#undef READ_I32
#undef BINARY_OP