using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir.syntax;

public class IRFunction : IRNode {
    public string Name { get; }
    public List<IRParameter> Parameters { get; }
    public IRTypeName ReturnType { get; }
    public IRFunctionKind Kind { get; }
    public bool IsMethod { get; }

    public List<IRStatement> Body { get; }

    public IRFunction (
        string name,
        List<IRParameter> parameters,
        IRTypeName returnType,
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
