using Judith.NET.analysis.syntax;

namespace Judith.NET.analysis;

public class JudithProgram {
    public required List<CompilerUnit> Units { get; init; }
    public required JudithNativeHeader NativeHeader { get; init; }
    public required List<JudithAssemblyHeader> Dependencies { get; init; }
}
