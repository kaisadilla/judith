using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public abstract class IRNode {
    public List<IRNode> Children { get; } = [];
}
