using Judith.NET.analysis.semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;
public class AssemblyHeader : IAssemblyHeader {
    public string Name { get; }

    public Dictionary<string, Symbol> Symbols { get; private init; }
    
    private AssemblyHeader (string name, Dictionary<string, Symbol> symbols) {
        Name = name;
        Symbols = symbols;
    }
}
