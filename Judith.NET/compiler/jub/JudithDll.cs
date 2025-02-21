using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jub;

public class JudithDll {
    public FunctionRefTable FunctionRefTable { get; private init; }
    public List<BinaryBlock> Blocks { get; private init; }

    public JudithDll (FunctionRefTable functionRefArray, List<BinaryBlock> blocks) {
        FunctionRefTable = functionRefArray;
        Blocks = blocks;
    }
}
