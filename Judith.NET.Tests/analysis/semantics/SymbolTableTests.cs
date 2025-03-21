using Judith.NET.analysis.semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.Tests.analysis.semantics;

public class SymbolTableTests : SymbolTable {
    public SymbolTableTests () : base(ScopeKind.Global, "", null, null) {}

    [Fact]
    public void SimpleRoot () {
        string[] tableNames = ["foo"];

        var tbl = CreateRootTable("test", tableNames);

        Assert.True(tbl.IsRootTable);
        Assert.True(tbl.Name == "foo");
        Assert.True(tbl.ChildTables.Count == 0);
    }
}
