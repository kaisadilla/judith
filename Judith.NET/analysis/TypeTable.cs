using Judith.NET.analysis.semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

public class TypeTable {
    public Dictionary<string, TypeSymbol> Types { get; private set; } = [];

    public void AddType (TypeSymbol type) {
        Types[type.FullyQualifiedName] = type;
    }
}
