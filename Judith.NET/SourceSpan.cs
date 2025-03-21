using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET;

public struct SourceSpan {
    public int Start { get; set; }
    public int End { get; set; }

    public readonly int Length => End - Start;

    public static SourceSpan None { get; } = new(-1, -1);

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
