using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public enum IRIdentifierKind {
    /// <summary>
    /// This identifier is a local inside this scope.
    /// </summary>
    Local,
    /// <summary>
    /// This identifier is a value that exists outside this scope.
    /// </summary>
    Global,
}

public class IRIdentifierExpression : IRExpression {
    public string Name { get; private set; }
    public IRIdentifierKind Kind { get; private set; }

    public IRIdentifierExpression (
        string name, IRIdentifierKind kind, IRTypeName type
    )
        : base(type)
    {
        Name = name;
        Kind = kind;
    }
}
