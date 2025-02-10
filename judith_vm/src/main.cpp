// judith_vm.cpp : Defines the entry point for the application.
//

#include "main.hpp"
#include <Chunk.hpp>
#include <ChunkReader.hpp>

int main () {
    std::unique_ptr<Chunk> chunk = readChunk();

    std::cout << "Chunk constants: " << chunk->constantCount << std::endl;
    return 0;
}
