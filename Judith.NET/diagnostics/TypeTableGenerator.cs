using Judith.NET.analysis;
using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.diagnostics;

public class TypeTableGenerator
{
    public List<object> TypeTable { get; private set; } = [];

    public void Analyze(SymbolTable table)
    {
        foreach (Symbol s in table.Symbols.Values)
        {
            if (s is TypeSymbol ts)
            {
                TypeTable.Add(new
                {
                    ts.Name,
                    ts.FullyQualifiedName,
                    ts.Kind,
                });
            }
        }

        foreach (SymbolTable inner in table.InnerTables.Values)
        {
            Analyze(inner);
        }
    }
}
