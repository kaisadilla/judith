using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jub;

/// <summary>
/// Contains a reference to a function. This reference indicates the index of
/// the block that contains the function, and the index of the function inside
/// said block.
/// </summary>
public class FunctionRef {
    public int Block { get; private init; }
    public int Index { get; private init; }

    public FunctionRef (int block, int index) {
        Block = block;
        Index = index;
    }

    public override string ToString () {
        return $"(Function at block {Block}, index {Index})";
    }
}
