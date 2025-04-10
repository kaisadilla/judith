using Judith.NET.analysis.semantics;
using Judith.NET.ir;
using Judith.NET.ir.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;
public interface IJudithHeader {
    IRAssemblyHeader IrHeader { get; }

    string Name { get; }

    /// <summary>
    /// Maps fully qualified names to types in this header.
    /// </summary>
    Dictionary<string, TypeSymbol> Types { get; }
    /// <summary>
    /// Maps fully qualified names to functions in this header.
    /// </summary>
    Dictionary<string, FunctionSymbol> Functions { get; }

    /// <summary>
    /// Maps types in this header to IR types in the IR header.
    /// </summary>
    Dictionary<TypeSymbol, IRType> TypeMap { get; }
    /// <summary>
    /// Maps functions in this header to IR functions in the IR header.
    /// </summary>
    Dictionary<FunctionSymbol, IRFunction> FunctionMap { get; }
}
