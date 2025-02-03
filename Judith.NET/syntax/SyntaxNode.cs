using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;
public abstract class SyntaxNode {
    [JsonProperty(Order = -1_000_000)]
    public SyntaxKind Kind { get; private set; }
    public SourceSpan Span { get; private set; }

    protected SyntaxNode (SyntaxKind kind) {
        Kind = kind;
        Span = new();
    }

    /// <summary>
    /// Sets the text span in the source code of this node.
    /// </summary>
    public void SetSpan (SourceSpan span) {
        Span = span;
    }

    public abstract override string ToString ();
}

public struct SourceSpan {
    public int Start { get; set; }
    public int End { get; set; }

    public SourceSpan () {
        Start = 0;
        End = 0;
    }

    public SourceSpan (int start, int end) {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Includes the span given into this current one. This means that,
    /// if the span given's start is lower than this one, this one's becomes
    /// the same as the span given. The same goes for the end, but in this case
    /// it's the highest end.
    /// </summary>
    /// <param name="span">The span to include.</param>
    public void Include (SourceSpan? span) {
        if (span is not null) {
            var s = (SourceSpan)span;
            Start = Math.Min(Start, s.Start);
            End = Math.Max(End, s.End);
        }
    }

    public void IncludeStart (int start) {
        End = Math.Min(Start, start);
    }

    public void IncludeEnd (int end) {
        End = Math.Max(End, end);
    }
}
