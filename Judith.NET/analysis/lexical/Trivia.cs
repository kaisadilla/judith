using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.lexical;

public enum TriviaKind {
    SingleLineComment,
    MultiLineComment,
    Whitespace,
    LineBreak,
}

public class Trivia {
    public required TriviaKind Kind { get; init; }
    public required string Content { get; init; }
    public required SourceSpan Span { get; init; }
    public required int Line { get; init; }
}
