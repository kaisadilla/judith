using Judith.NET.analysis.lexical;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;
public class Literal : SyntaxNode {
    public TokenKind TokenKind { get; private set; }
    public string Source { get; private set; }

    public Token? RawToken { get; init; }

    /// <summary>
    /// Builds a literal from a literal token (that is, types "string" or "number").
    /// </summary>
    /// <param name="token"></param>
    public Literal (Token token) : base(SyntaxKind.Literal) {
        if (IsValidTokenKind(token.Kind) == false) throw new Exception(
            $"Token kind '{Token.GetTokenName(token.Kind)}' cannot be used " +
            $"to build a literal."
        );

        RawToken = token;
        TokenKind = token.Kind;
        Source = token.Lexeme;
    }

    /// <summary>
    /// Builds a literal from a description of what a token for it would have.
    /// </summary>
    /// <param name="literalKind">Either "string" or "number"</param>
    /// <param name="source">The Judith string that represents that literal.</param>
    public Literal (TokenKind literalKind, string source) : base(SyntaxKind.Literal) {
        if (IsValidTokenKind(literalKind) == false) throw new Exception(
            $"Token kind '{Token.GetTokenName(literalKind)}' cannot be used " +
            $"to build a literal."
        );

        TokenKind = literalKind;
        Source = source;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }

    private bool IsValidTokenKind (TokenKind kind) {
        return kind == TokenKind.String
            || kind == TokenKind.Number
            || kind == TokenKind.KwTrue
            || kind == TokenKind.KwFalse
            || kind == TokenKind.KwNull
            || kind == TokenKind.KwUndefined;
    }

    public override string ToString () {
        return $"{Kind} ({Source}) [Line: {Line}, Span: {Span.Start} - {Span.End}]";
    }
}
