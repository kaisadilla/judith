using Judith.NET.analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jub;

public class BinaryBlock {
    public string Name { get; private init; }
    public ConstantTable ConstantTable { get; private set; } = new();
    public List<BinaryFunction> Functions { get; private set; } = new();
    public bool HasImplicitFunction { get; set; } = false;

    public BinaryBlock (string name) {
        Name = name;
    }
}

public class BinaryFunction {
    public string Name { get; private init; }
    public int NameIndex { get; private init; }
    public List<FunctionParameter> Parameters { get; private set; } = new();
    /// <summary>
    /// The maximum amount of locals that this function may add.
    /// </summary>
    public int MaxLocals { get; set; } = 0;
    public Chunk Chunk { get; private set; } = new();

    /// <summary>
    /// The amount of parameters defined in this function.
    /// </summary>
    public int Arity => Parameters.Count;

    public BinaryFunction (BinaryBlock file, string name) {
        Name = name;
        NameIndex = file.ConstantTable.WriteStringASCII(Name);
    }
}

public class FunctionParameter {
    public TypeInfo Type { get; private init; }
    public string Name { get; private init; }
    public int NameIndex { get; private init; }

    public FunctionParameter (BinaryBlock file, TypeInfo type, string name) {
        Type = type;
        Name = name;
        NameIndex = file.ConstantTable.WriteStringASCII(Name);
    }
}