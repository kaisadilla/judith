using Judith.NET.analysis.lexical;
using Judith.NET.analysis.syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.message;

public class MessageSource {
    public int? AsLine { get; private init; } = null;
    public Token? AsToken { get; private init; } = null;
    public SyntaxNode? AsNode { get; private init; } = null;

    public object? Value => (object?)AsLine
        ?? (object?)AsToken
        ?? (object?)AsNode
        ?? throw new InvalidUnionException();

    public MessageSource (int line) {
        AsLine = line;
    }

    public MessageSource (Token token) {
        AsToken = token;
    }

    public MessageSource (SyntaxNode node) {
        AsNode = node;
    }

    /// <summary>
    /// Returns the line in which the message originated, regardless of the
    /// source of the message.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidUnionException"></exception>
    public int GetLine () {
        return AsLine.HasValue ? AsLine.Value
            : AsToken?.Line ?? AsNode?.Line ?? throw new InvalidUnionException();
    }
}
