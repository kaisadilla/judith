#pragma once

#include "root.hpp"

struct Block;
struct AssemblyFile;

std::string disassembleAssembly (const AssemblyFile& file);