#include "runtime/Assembly.hpp"
#include "VM.hpp"
#include "runtime/FuncRef.hpp"

u_ptr<Assembly> Assembly::buildCompletely (VM& vm, AssemblyFile& file) {
    u_ptr<Assembly> assembly = make_u<Assembly>(file.funcRefs.size());

    assembly->nameTable.reserve(file.nameTable.count);
    assembly->blocks.reserve(file.blocks.size());

    for (size_t i = 0; i < file.nameTable.count; i++) {
        assembly->nameTable.push_back(
            // Create a StringObject in the interned string table from the
            // string that exists inside the AssemblyFile's nameTable, and
            // store a weak pointer to it in the nameTable vector.
            vm.getInternedStringTable().getStringObject(file.nameTable.strings[i])
        );
    }

    for (size_t b = 0; b < file.blocks.size(); b++) {
        assembly->blocks.emplace_back(
            Block::buildCompletely(vm, assembly.get(), file.blocks[b])
        );
    }
    
    for (size_t fr = 0; fr < file.funcRefs.size(); fr++) {
        switch (file.funcRefs[fr]->refType) {
        case ItemRef::TYPE_INTERNAL: {
            const InternalRef& iref = *static_cast<InternalRef*>(
                file.funcRefs.at(fr).get()
            );

            auto& func = assembly->blocks.at(iref.block)->functions.at(iref.index);

            assembly->functionRefs.references[fr] = new InternalFuncRef(
                *assembly, fr, func.get()
            );

            assembly->functionRefs.functions[fr] = &FuncRef::callFunction;

            break;
        }
        case ItemRef::TYPE_NATIVE: {
            const NativeRef& nref = *static_cast<NativeRef*>(
                file.funcRefs.at(fr).get()
            );

            const auto& func = vm.getNativeAssembly().getFunc(nref.index);

            assembly->functionRefs.references[fr] = new NativeFuncRef(
                *assembly, fr, func
            );

            assembly->functionRefs.functions[fr] = &FuncRef::callFunction;

            break;
        }
        case ItemRef::TYPE_EXTERNAL: {
            throw "Not implemented yet!";
        }
        }
    }

    return std::move(assembly);
}

void Assembly::bindReferences () {
    for (auto& block : blocks) {
        block->assembly = this;

        for (auto& func : block->functions) {
            func->funcRefs = &functionRefs;
            func->chunk.stringTable = &block->stringTable;
        }
    }
}
