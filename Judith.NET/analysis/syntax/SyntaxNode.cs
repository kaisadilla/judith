﻿using Newtonsoft.Json;
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
    public bool IsAutoGenerated { get; init; } = false;

    /// <summary>
    /// Contains references to all the nodes that are children of this one.
    /// </summary>
    [JsonIgnore]
    public List<SyntaxNode> Children { get; private set; } = new();

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

    public void SetLine (int line) {
        Line = line;
    }

    public abstract void Accept (SyntaxVisitor visitor);
    public abstract T? Accept<T> (SyntaxVisitor<T> visitor);

    public override string ToString () {
        return $"{Kind} [Line: {Line}, Span: {Span.Start} - {Span.End}]";
    }
}
