namespace Judith.NET.codegen.jasm;

public class JasmAssembly {
    public required int JudithVersion { get; init; }
    public required Version Version { get; init; }
    public StringTable NameTable { get; init; } = new();
    public JasmRefTable TypeRefTable { get; init; } = new();
    public JasmRefTable FunctionRefTable { get; init; } = new();
    public List<JasmBlock> Blocks { get; init; } = [];
}
