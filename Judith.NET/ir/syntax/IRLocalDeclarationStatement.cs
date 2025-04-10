using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRLocalDeclarationStatement : IRStatement {
    public string Name { get; private set; }
    public IRTypeName Type { get; private set; }
    public bool IsFinal { get; private init; }
    public bool IsImmutable { get; private init; }
    public IRExpression? Initialization { get; private set; }

    public IRLocalDeclarationStatement (
        string name,
        IRTypeName type,
        bool isFinal,
        bool isImmutable,
        IRExpression? initialization
    ) {
        Name = name;
        Type = type;
        IsFinal = isFinal;
        IsImmutable = isImmutable;
        Initialization = initialization;
    }
}
