using Judith.NET.analysis;
using Judith.NET.analysis.semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.codegen.jasm;

public class JasmBlock {
    public int NameIndex { get; private init; }
    public StringTable StringTable { get; private set; } = new();
    public List<JasmFunction> FunctionTable { get; private set; } = new();

    public JasmBlock (int nameIndex) {
        NameIndex = nameIndex;
    }
}

public class JasmFunction {
    public int NameIndex { get; private init; }
    public List<JasmFunctionParameter> Parameters { get; private set; } = new();
    /// <summary>
    /// The maximum amount of locals that this function may add.
    /// </summary>
    public int MaxLocals { get; set; } = 0;
    public Chunk Chunk { get; private set; } = new();

    /// <summary>
    /// The amount of parameters defined in this function.
    /// </summary>
    public int Arity => Parameters.Count;

    public JasmFunction (int nameIndex) {
        NameIndex = nameIndex;
    }
}

public class JasmFunctionParameter {
    public int NameIndex { get; private init; }
    public int TypeIndex { get; private init; }

    public JasmFunctionParameter (int nameIndex, int typeIndex) {
        NameIndex = nameIndex;
        TypeIndex = typeIndex;
    }
}