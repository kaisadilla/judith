using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.codegen.jasm;

public class JasmAssembly {
    public Version Version { get; private init; }
    public FunctionRefTable FunctionRefTable { get; private init; }
    public List<BinaryBlock> Blocks { get; private init; }

    public JasmAssembly (FunctionRefTable functionRefArray, List<BinaryBlock> blocks) {
        FunctionRefTable = functionRefArray;
        Blocks = blocks;
    }
}
