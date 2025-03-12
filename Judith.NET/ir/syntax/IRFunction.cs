using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRFunction : IRNode {
    public string Name { get; private init; }
    public List<IRParameter> Parameters { get; private init; }
    public string ReturnType { get; private init; }
    public IRFunctionKind Kind { get; private init; }
    public bool IsMethod { get; private init; }

    public List<IRStatement> Body { get; private init; }

    public IRFunction (
        string name,
        List<IRParameter> parameters,
        string returnType,
        List<IRStatement> body,
        IRFunctionKind kind,
        bool isMethod
    ) {
        Name = name;
        Parameters = parameters;
        ReturnType = returnType;
        Body = body;
        Kind = kind;
        IsMethod = isMethod;
    }
}
