using Judith.NET.ir.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir;

public class IRAssemblyHeader {
    public required Dictionary<string, IRType> Types { get; init; }
    public required Dictionary<string, IRFunction> Functions { get; init; }
}
