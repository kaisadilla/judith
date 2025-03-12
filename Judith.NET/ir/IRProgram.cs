using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir;

public class IRProgram {
    public List<IRBlock> Blocks { get; private set; } = [];
    public required IRNativeHeader NativeHeader { get; init; }
    public required List<IRAssemblyHeader> Dependencies { get; init; }
}
