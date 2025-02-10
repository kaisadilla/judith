#include "utils.hpp"
#include <fstream>

std::vector<byte> readBinaryFile (const std::string& filePath) {
    std::ifstream in(filePath, std::ios::in | std::ios::binary);
    std::vector<byte> data(
        (std::istreambuf_iterator<char>(in)),
        std::istreambuf_iterator<char>()
    );

    return data;
}
