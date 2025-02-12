// judith_vm.cpp : Defines the entry point for the application.
//

#include "main.hpp"
#include "debug/chunk.hpp"
#include <Chunk.hpp>
#include <ChunkReader.hpp>
#include <VM.hpp>

int main () {
    Chunk chunk = readChunk();

    std::cout << "\n\n===== DISASSEMBLE =====" << std::endl;
    std::string dump = disassembleChunk(chunk);
    std::cout << dump << std::endl;

    std::cout << "\n\n===== EXECUTION =====" << std::endl;

    VM vm;
    vm.interpret(chunk);

    getchar();
    return 0;
}
