// judith_vm.cpp : Defines the entry point for the application.
//

#include "main.hpp"
#include "debug/disassembly.hpp"
#include <executable/Block.hpp>
#include <executable/Function.hpp>
#include <BlockReader.hpp>
#include <VM.hpp>

int main () {
    Block block = readBlock();

    std::cout << "\n\n===== DISASSEMBLE =====" << std::endl;
    std::string dump = disassembleBlock(block);
    std::cout << dump << std::endl;

    std::cout << "\n\n===== EXECUTION =====" << std::endl;

    //VM vm;
    //vm.interpret(block.functions[0].chunk);

    //getchar();
    return 0;
}
