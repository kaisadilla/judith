using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRLocalDeclarationStatement : IRStatement {
    public string Name { get; private set; }
    public string Type { get; private set; }
    public IRMutability Mutability { get; private set; }
    public IRExpression? Initialization { get; private set; }

    public IRLocalDeclarationStatement (
        string name, string type, IRMutability mutability, IRExpression? initialization
    ) {
        Name = name;
        Type = type;
        Mutability = mutability;
        Initialization = initialization;
    }
}
