// judith_vm.cpp : Defines the entry point for the application.
//

#include "main.hpp"
#include "debug/chunk.hpp"
#include <Chunk.hpp>
#include <ChunkReader.hpp>

int main () {
    std::unique_ptr<Chunk> chunk = readChunk();

    std::string dump = disassembleChunk(*chunk);

    std::cout << dump << std::endl;
    //std::cout << "Chunk constants: " << chunk->constantCount << std::endl;

    getchar();
    return 0;
}
