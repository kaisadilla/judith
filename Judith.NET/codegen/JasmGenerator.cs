using Judith.NET.codegen.jasm;
using Judith.NET.ir;
using Judith.NET.ir.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Judith.NET.codegen;

public class JasmGenerator {
    /// <summary>
    /// The program being compiled.
    /// </summary>
    public IRProgram Program { get; }
    public IRProgramResolver Resolver { get; }

    /// <summary>
    /// The assembly being built by this generator.
    /// </summary>
    public JasmAssembly Assembly { get; private set; }

    public JasmGenerator (IRProgram program) {
        Program = program;
        Resolver = new(Program);

        Assembly = new() {
            JudithVersion = 0,
            Version = new(1, 1, 1, 1),
        };
    }

    public void Generate () {
        CollectReferences();

        foreach (var block in Program.Blocks) {
            GenerateBlock(block);
        }
    }

    /// <summary>
    /// Returns the index of the type reference with the name given. If the
    /// assembly doesn't have a reference to that type, a new reference is
    /// created for that type.
    /// If the type's name cannot be resolved, an exception is thrown.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <returns></returns>
    /// <exception cref="InvalidIRProgramException"></exception>
    public int GetTypeReferenceIndex (string name) {
        // If the name is already in the type reference table, return its index.
        if (Assembly.TypeRefTable.TryGetRefIndex(name, out int typeRefIndex)) {
            return typeRefIndex;
        }

        // Try to find the type in the native header.
        if (Program.NativeHeader.TryGetTypeIndex(name, out int nativeIndex)) {
            // If found, create a reference to it.
            var typeRef = new JasmNativeRef(nativeIndex);
            // Add it to the reference table.
            Assembly.TypeRefTable.Add(name, typeRef);

            // And return its index (which is the last)
            return Assembly.TypeRefTable.Table.Count - 1;
        }

        // Try to find the type inside the dependencies.
        // TODO:

        // If it isn't found by now, the IR program is invalid.
        throw new InvalidIRProgramException($"Cannot resolve type name '{name}'");
    }

    /// <summary>
    /// Returns the index of the function reference with the name given.
    /// If the assembly doesn't have a reference to that function, a new 
    /// reference is created for that function.
    /// If the function's name cannot be resolved, an exception is thrown.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <returns></returns>
    /// <exception cref="InvalidIRProgramException"></exception>
    public int GetFunctionReferenceIndex (string name) {
        // If the name is already in the function reference table, return its index.
        if (Assembly.FunctionRefTable.TryGetRefIndex(name, out int index)) {
            return index;
        }

        // Try to find the function in the native header.
        if (Program.NativeHeader.TryGetFunctionIndex(name, out int nativeIndex)) {
            // If found, create a reference to it.
            var funcRef = new JasmNativeRef(nativeIndex);
            // Add it to the reference table.
            Assembly.FunctionRefTable.Add(name, funcRef);

            // And return its index (which is the last)
            return Assembly.FunctionRefTable.Table.Count - 1;
        }

        // Try to find the function inside the dependencies.
        // TODO:

        // If it isn't found by now, the IR program is invalid.
        throw new InvalidIRProgramException($"Cannot resolve function name '{name}'");
    }

    /// <summary>
    /// Adds all the internal references to the assembly's ref tables.
    /// </summary>
    private void CollectReferences () {
        for (int b = 0; b < Program.Blocks.Count; b++) {
            var block = Program.Blocks[b];

            for (int f = 0; f < block.Functions.Count; f++) {
                var func = block.Functions[f];

                Assembly.FunctionRefTable.Add(func.Name, new JasmInternalRef(b, f));
            }

            for (int t = 0; t < block.Types.Count; t++) {
                var type = block.Types[t];

                Assembly.TypeRefTable.Add(type.Name, new JasmInternalRef(b, t));
            }
        }
    }

    private void GenerateBlock (IRBlock irBlock) {
        var nameIndex = Assembly.NameTable.GetStringIndex(irBlock.Name);

        var jasmBlock = new JasmBlock(nameIndex);
        Assembly.Blocks.Add(jasmBlock);

        foreach (var func in irBlock.Functions) {
            GenerateFunction(jasmBlock, func);
        }
    }

    private void GenerateFunction (JasmBlock jasmBlock, IRFunction irFunc) {
        var funcCompiler = new JasmFunctionCompiler(this, jasmBlock, irFunc);
        funcCompiler.CompileFunction();

        jasmBlock.FunctionTable.Add(funcCompiler.JasmFunction);
    }
}
