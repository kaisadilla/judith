#include "VM.hpp"
#include "jal/opcodes.hpp"

#define READ_BYTE() (*(ip++))
#define READ_I32() (*(ip++) | (*(ip++) << 8) | (*(ip++) << 16) | (*(ip++) << 24))

#define BINARY_OP(op) \
    do { \
        f64 b = popValue().asFloat64; \
        f64 a = popValue().asFloat64; \
        pushValue({ .asFloat64 = (a op b) }); \
    } while (false)

InterpretResult VM::interpret (const Chunk& chunk) {
    byte* ip = chunk.code;

    while (true) {
#ifdef DEBUG_DUMP_STACK
        std::cout << "stack: [";
        for (Value* slot = stack; slot < stackTop; slot++) {
            std::cout << slot->as.float64 << ", ";
        }
        std::cout << "]" << std::endl;
#endif

        byte instruction = *(ip++);
        switch (instruction) {
        case OpCode::NOOP:
            break;

        case OpCode::CONST:
            pushValue(chunk.constants[READ_BYTE()]);

            break;

        case OpCode::CONST_LONG:
            pushValue(chunk.constants[READ_I32()]);

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

        case OpCode::PRINT:
            printValue(popValue());
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