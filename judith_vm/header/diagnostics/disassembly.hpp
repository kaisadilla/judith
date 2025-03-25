#pragma once

#include "root.hpp"

class Block;
struct AssemblyFile;

std::string disassembleAssembly (const AssemblyFile& file);