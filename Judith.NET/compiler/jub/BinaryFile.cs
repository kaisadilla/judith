using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jub;

public class BinaryFile {
    public ConstantTable ConstantTable { get; private set; } = new();
    public List<BinaryFunction> Functions { get; private set; } = new();
    public int EntryPoint = -1; // -1 = No entry point.
}

public class BinaryFunction {
    public Chunk Chunk { get; private set; } = new();
}