using Judith.NET.analysis.semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

public interface ICompilation {
    public SymbolTable SymbolTable { get; }
    public TypeTable TypeTable { get; }
}
