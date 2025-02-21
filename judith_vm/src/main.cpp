// judith_vm.cpp : Defines the entry point for the application.
//

#include "main.hpp"
#include "debug/disassembly.hpp"
#include <executable/Assembly.hpp>
#include <executable/Function.hpp>
#include <BlockReader.hpp>
#include <VM.hpp>

int main () {
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
