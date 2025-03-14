#pragma once

#include "root.hpp"

class Assembly;
class VM;

Assembly readAssembly(VM* vm, const char* filePath);