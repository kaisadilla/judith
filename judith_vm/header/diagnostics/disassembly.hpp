#pragma once

#include "root.hpp"

struct Block;
class Assembly;

std::string disassembleAssembly (Assembly& assembly);
std::string disassembleBlock (Block& block);