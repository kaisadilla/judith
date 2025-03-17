using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public abstract class IRType : IRNode {
    public string Name { get; private init; }

    protected IRType (string name) {
        Name = name;
    }
}

public class IRPseudoType : IRType {
    public IRPseudoType (string name) : base(name) {}
}