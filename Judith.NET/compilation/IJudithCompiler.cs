using Judith.NET.analysis;
using Judith.NET.analysis.lexical;
using Judith.NET.analysis.syntax;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compilation;

public interface IJudithCompiler {
    MessageContainer Messages { get; }
    List<Token>? Tokens { get; }
    List<SyntaxNode>? Ast { get; }
    List<CompilerUnit>? CompilerUnits { get; }
    JudithCompilation? Compilation { get; }
}
