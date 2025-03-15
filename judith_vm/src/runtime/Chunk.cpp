#include "runtime/Chunk.hpp"
#include "runtime/Block.hpp"

u_ptr<Chunk> Chunk::build(
    const Block& block, const std::vector<byte>& binaryChunk
) {
    u_ptr<Chunk> chunk = make_u<Chunk>(Chunk{ .stringTable = &block.stringTable });

    chunk->size = binaryChunk.size();

    chunk->code = make_u<byte[]>(chunk->size);
    memcpy(chunk->code.get(), binaryChunk.data(), chunk->size);

    return std::move(chunk);
}
