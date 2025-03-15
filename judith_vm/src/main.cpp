// judith_vm.cpp : Defines the entry point for the application.
//

#include "main.hpp"
#include "diagnostics/disassembly.hpp"
#include "runtime/Assembly.hpp"
#include <VM.hpp>
#include <filesystem>
#include <fstream>

#if defined(_WIN32) || defined(_WIN64)
#define NOMINMAX
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#endif

namespace fs = std::filesystem;
using Path = fs::path;

int main (int argc, char *argv[]) {
    auto args = std::vector<std::string>(argv + 1, argv + argc);

#if defined(_WIN32) || defined(_WIN64)
    SetConsoleOutputCP(CP_UTF8);
#endif

    std::streambuf* cmdBuf = std::cout.rdbuf();

    Path path = "res/test.jdll";
    u_ptr<Path> outFilePath(nullptr);
    u_ptr<std::ofstream> outFileStream(nullptr);

    if (args.size() == 0) {
        std::cout << "no arguments - juvm test mode\n";
    }
    if (args.size() >= 1) {
        auto execPath = fs::current_path();
        Path subdir = args[0];

        path = execPath / subdir;
    }
    if (args.size() >= 2) {
        outFilePath = make_u<Path>(args[1]);
    }

    Path directory = path.parent_path();
    Path fileName = path.filename();

    VM vm(directory);
    vm.start(fileName);
    
    //Assembly exec = readAssembly(path.string().c_str());

    //std::cout << "\n\n===== DISASSEMBLE =====" << std::endl;
    //std::string dump = disassembleAssembly(
    //    vm.getAssemblyFile(fileName.stem().string().c_str())
    //);
    //std::cout << dump << std::endl;





    //Assembly assembly = readAssembly(path.string().c_str());
    //
    //std::cout << "\n\n===== DISASSEMBLE =====" << std::endl;
    //std::string dump = disassembleBlock(assembly.blocks[0]);
    //std::cout << dump << std::endl;
    //
    //std::cout << "\n\n===== EXECUTION =====" << std::endl;
    //
    //if (outFilePath != nullptr) {
    //    Path dir = outFilePath->parent_path();
    //    if (fs::exists(dir) == false) {
    //        fs::create_directories(dir);
    //    }
    //
    //    outFileStream = make_u<std::ofstream>(outFilePath->c_str());
    //    std::cout.rdbuf(outFileStream->rdbuf());
    //}
    //
    //VM vm;
    //vm.interpret(assembly);
    //
    //if (outFilePath != nullptr) {
    //    std::cout.rdbuf(cmdBuf);
    //}
    //
    //std::cout << std::endl;

    //getchar();
    return 0;
}
