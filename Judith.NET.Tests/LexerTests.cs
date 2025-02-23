using Judith.NET.analysis.syntax;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.Tests;

public class LexerTests {

    [Theory]
    [InlineData("50")]
    [InlineData("121.30")]
    [InlineData("1_721")]
    [InlineData("771_284")]
    [InlineData("-23")]
    [InlineData("-.75")]
    [InlineData("-200.1")]
    public void CorrectNumberLiteral (string literal) {
        Lexer lexer = Tokenize(literal);
        Assert.True(lexer.Messages.Errors.Count == 0);
        Assert.True(lexer.Tokens != null);
        Assert.True(lexer.Tokens.Count == 2);
        Assert.True(lexer.Tokens![0].Lexeme == literal);
        Assert.True(lexer.Tokens[^1].Kind == TokenKind.EOF);
    }

    [Theory]
    [InlineData("371.")]
    [InlineData("._61")]
    [InlineData(".882")]
    [InlineData("150__0")]
    //[InlineData("15_")]
    //[InlineData("23.45.67")]
    public void InvalidNumberLiteral (string literal) {
        Lexer lexer = Tokenize(literal);
        Assert.True(lexer.Tokens != null);
        Assert.True(lexer.Tokens.Count != 2 || lexer.Tokens[0].Kind != TokenKind.Number);
        Assert.True(lexer.Tokens[^1].Kind == TokenKind.EOF);
    }

    [Theory]
    [InlineData("if", TokenKind.KwIf)]
    [InlineData("while", TokenKind.KwWhile)]
    [InlineData("true", TokenKind.KwTrue)]
    public void CorrectKeyword (string literal, TokenKind keywordKind) {
        Lexer lexer = Tokenize(literal);
        Assert.True(lexer.Messages.Errors.Count == 0);
        Assert.True(lexer.Tokens != null);
        Assert.True(lexer.Tokens.Count == 2);
        Assert.True(lexer.Tokens[0].Kind == keywordKind);
        Assert.True(lexer.Tokens[^1].Kind == TokenKind.EOF);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("josh")]
    [InlineData("\\while")]
    [InlineData("iffoo")]
    [InlineData("whilebar")]
    [InlineData("elsethen")]
    [InlineData("novedo")]
    public void CorrectIdentifier (string identifier) {
        Lexer lexer = Tokenize(identifier);
        Assert.True(lexer.Messages.Errors.Count == 0);
        Assert.True(lexer.Tokens != null);
        Assert.True(lexer.Tokens.Count == 2);
        Assert.True(lexer.Tokens[0].Kind == TokenKind.Identifier);
        Assert.True(lexer.Tokens[^1].Kind == TokenKind.EOF);
    }

    [Theory]
    [InlineData("+", TokenKind.Plus)]
    [InlineData("-", TokenKind.Minus)]
    [InlineData("*", TokenKind.Asterisk)]
    [InlineData("/", TokenKind.Slash)]
    [InlineData("=", TokenKind.Equal)]
    [InlineData("==", TokenKind.EqualEqual)]
    [InlineData("!=", TokenKind.BangEqual)]
    [InlineData("===", TokenKind.EqualEqualEqual)]
    [InlineData("!==", TokenKind.BangEqualEqual)]
    [InlineData("<", TokenKind.Less)]
    [InlineData("<=", TokenKind.LessEqual)]
    [InlineData(">", TokenKind.Greater)]
    [InlineData(">=", TokenKind.GreaterEqual)]
    public void CorrectOperator (string src, TokenKind keywordKind) {
        Lexer lexer = Tokenize(src);
        Assert.True(lexer.Messages.Errors.Count == 0);
        Assert.True(lexer.Tokens != null);
        Assert.True(lexer.Tokens.Count == 2);
        Assert.True(lexer.Tokens[0].Kind == keywordKind);
        Assert.True(lexer.Tokens[^1].Kind == TokenKind.EOF);
    }

    [Fact]
    public void EmptyInput () {
        Lexer lexer = Tokenize("");
        Assert.True(lexer.Messages.Errors.Count == 0);
        Assert.True(lexer.Tokens != null);
        Assert.True(lexer.Tokens.Count == 1);
        Assert.True(lexer.Tokens[^1].Kind == TokenKind.EOF);
    }

    [Theory]
    [InlineData("\"unterminated")]
    [InlineData("`unterminated")]
    public void UnterminatedString (string src) {
        Lexer lexer = Tokenize(src);
        Assert.True(lexer.Messages.Errors.Count == 1);
        Assert.True(lexer.Messages.Errors[0].Code == (int)MessageCode.UnterminatedString);
        Assert.True(lexer.Tokens != null);
        Assert.True(lexer.Tokens.Count == 1);
        Assert.True(lexer.Tokens[^1].Kind == TokenKind.EOF);
    }

    private Lexer Tokenize (string src) {
        Lexer lexer = new(src);
        lexer.Tokenize();
        return lexer;
    }
}
