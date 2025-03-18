using Judith.NET.analysis.lexical;
using Judith.NET.analysis.syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.message;

public class MessageSource {
    [JsonIgnore]
    private int? _asLine = null;
    [JsonIgnore]
    private Token? _asToken = null;
    [JsonIgnore]
    private SyntaxNode? _asNode = null;

    public Type Type { get; private init; }

    [JsonIgnore]
    public int AsLine => _asLine ?? throw new InvalidUnionAccessException(
        nameof(AsLine), nameof(MessageSource), Type.Name
    );

    [JsonIgnore]
    public Token AsToken => _asToken ?? throw new InvalidUnionAccessException(
        nameof(AsToken), nameof(MessageSource), Type.Name
    );

    [JsonIgnore]
    public SyntaxNode AsNode => _asNode ?? throw new InvalidUnionAccessException(
        nameof(AsNode), nameof(MessageSource), Type.Name
    );

    public object? Value => (object?)_asLine
        ?? (object?)_asToken
        ?? (object?)_asNode
        ?? throw new InvalidUnionException();

    public MessageSource (int line) {
        Type = typeof(int);
        _asLine = line;
    }

    public MessageSource (Token token) {
        Type = typeof(Token);
        _asToken = token;
    }

    public MessageSource (SyntaxNode node) {
        Type = typeof(SyntaxNode);
        _asNode = node;
    }

    public int GetLine () {
        if (_asLine != null) return _asLine.Value;
        if (_asToken != null) return _asToken.Line;
        if (_asNode != null) return _asNode.Line;
        throw new InvalidUnionException();
    }
}
