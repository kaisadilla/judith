using Judith.NET.analysis.semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;
public class JudithAssemblyHeader : IJudithHeader {
    public string Name { get; }

    public Dictionary<string, TypeSymbol> Types { get; private init; }
    public Dictionary<string, FunctionSymbol> Functions { get; private init; }

    public JudithAssemblyHeader (
        string name,
        Dictionary<string, TypeSymbol> types,
        Dictionary<string, FunctionSymbol> functions
    ) {
        Name = name;
        Types = types;
        Functions = functions;
    }
}
