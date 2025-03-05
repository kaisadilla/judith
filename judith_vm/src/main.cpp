// judith_vm.cpp : Defines the entry point for the application.
//

#include "main.hpp"
#include "debug/disassembly.hpp"
#include <executable/Assembly.hpp>
#include <executable/Function.hpp>
#include <BlockReader.hpp>
#include <VM.hpp>

#if defined(_WIN32) || defined(_WIN64)
#define NOMINMAX
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#endif

int main () {
#if defined(_WIN32) || defined(_WIN64)
    SetConsoleOutputCP(CP_UTF8);
#endif

    Assembly assembly = readAssembly();

    std::cout << "\n\n===== DISASSEMBLE =====" << std::endl;
    std::string dump = disassembleBlock(assembly.blocks[0]);
    std::cout << dump << std::endl;

    std::cout << "\n\n===== EXECUTION =====" << std::endl;

    VM vm;
    vm.interpret(assembly);

    //getchar();
    return 0;
}
