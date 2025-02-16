using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;
public abstract class SyntaxNode {
    [JsonProperty(Order = -1_000_000)]
    public SyntaxKind Kind { get; private set; }
    public SourceSpan Span { get; private set; }
    public int Line { get; private set; } = -1;

    /// <summary>
    /// Contains references to all the nodes that are children of this one.
    /// </summary>
    [JsonIgnore]
    public List<SyntaxNode> Children { get; private set; } = new();

    protected SyntaxNode(SyntaxKind kind)
    {
        Kind = kind;
        Span = new();
    }

    /// <summary>
    /// Sets the text span in the source code of this node.
    /// </summary>
    public void SetSpan(SourceSpan span)
    {
        Span = span;
    }

    public void SetLine(int line)
    {
        Line = line;
    }

    public abstract void Accept(SyntaxVisitor visitor);

    public abstract override string ToString();

    /// <summary>
    /// Given an object, returns a string that can be used in ToString() methods
    /// to represent the information given with correct indentation.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    protected static string Stringify(object obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented)
            .Replace("\\r\\n", "\r\n");
    }

    protected static string Stringify<T>(List<T> list)
    {
        return JsonConvert.SerializeObject(list, Formatting.Indented)
            .Replace("\\r\\n", "\r\n");
    }
}

public struct SourceSpan
{
    public int Start { get; set; }
    public int End { get; set; }

    public SourceSpan()
    {
        Start = 0;
        End = 0;
    }

    public SourceSpan(int start, int end)
    {
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
    public void Include(SourceSpan? span)
    {
        if (span is not null)
        {
            var s = (SourceSpan)span;
            Start = Math.Min(Start, s.Start);
            End = Math.Max(End, s.End);
        }
    }

    public void IncludeStart(int start)
    {
        End = Math.Min(Start, start);
    }

    public void IncludeEnd(int end)
    {
        End = Math.Max(End, end);
    }
}
