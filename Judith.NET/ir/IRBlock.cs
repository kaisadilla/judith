using Judith.NET.ir.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir;

public class IRBlock {
    public string Name { get; }
    public List<IRFunction> Functions { get; } = [];
    public List<IRType> Types { get; } = [];

    private readonly Dictionary<string, IRFunction> _funcDictionary = [];

    public IRBlock (string name) {
        Name = name;
    }

    public void AddFunction (IRFunction function) {
        Functions.Add(function);
        _funcDictionary[function.Name] = function;
    }

    public bool TryGetFunction (string name, out IRFunction? function) {
        return _funcDictionary.TryGetValue(name, out function);
    }
}
