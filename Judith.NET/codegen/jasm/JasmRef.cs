using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.codegen.jasm;

public abstract class JasmRef {
    public enum Kind {
        Internal = 0,
        Native = 1,
        External = 2,
    }

    public abstract Kind RefType { get; }
}

/// <summary>
/// Contains a reference to a function. This reference indicates the index of
/// the block that contains the function, and the index of the function inside
/// said block.
/// </summary>
public class JasmInternalRef : JasmRef {
    public override Kind RefType => Kind.Internal;

    public int Block { get; private init; }
    public int Index { get; private init; }

    public JasmInternalRef (int block, int index) {
        Block = block;
        Index = index;
    }

    public override string ToString () {
        return $"(Function at block {Block}, index {Index})";
    }
}

public class JasmNativeRef : JasmRef {
    public override Kind RefType => Kind.Native;

    public int Index { get; private init; }

    public JasmNativeRef (int index) {
        Index = index;
    }
}

public class JasmExternalRef : JasmRef {
    public override Kind RefType => Kind.External;

    /// <summary>
    /// The name table that contains the names used in this reference.
    /// </summary>
    [JsonIgnore]
    public StringTable NameTable { get; private init; }

    public int BlockName { get; private init; }
    public int ItemName { get; private init; }

    public JasmExternalRef (StringTable nameTable, int blockName, int indexName) {
        NameTable = nameTable;
        BlockName = blockName;
        ItemName = indexName;
    }
}
