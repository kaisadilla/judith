using Judith.NET.analysis.semantics;
using Judith.NET.ir;
using Judith.NET.ir.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;
public class JudithAssemblyHeader : IJudithHeader {
    public IRAssemblyHeader IrHeader { get; }
    public string Name { get; }

    public Dictionary<string, TypeSymbol> Types { get; }
    public Dictionary<string, FunctionSymbol> Functions { get; }

    public Dictionary<TypeSymbol, IRType> TypeMap { get; }
    public Dictionary<FunctionSymbol, IRFunction> FunctionMap { get; }

    public JudithAssemblyHeader () {
        throw new NotImplementedException();
    }
}
