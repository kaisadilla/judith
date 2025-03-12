using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRParameter : IRNode {
    public string Name { get; private init; }
    public string Type { get; private init; }
    public IRMutability Mutability { get; private init; }

    public IRParameter (string name, string type, IRMutability mutability) {
        Name = name;
        Type = type;
        Mutability = mutability;
    }
}
