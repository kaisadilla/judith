using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRParameter : IRNode {
    public string Name { get; private init; }
    public IRTypeName Type { get; private init; }
    public bool IsFinal { get; private init; }

    public IRParameter (string name, IRTypeName type, bool isFinal) {
        Name = name;
        Type = type;
        IsFinal = isFinal;
    }
}
