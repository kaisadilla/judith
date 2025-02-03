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
    [InlineData(".882")]
    [InlineData("771_284")]
    [InlineData("-23")]
    [InlineData("-.75")]
    [InlineData("-200.1")]
    public void CorrectNumberLiteral (string literal) {
        Lexer lexer = SmallCompile(literal);
        Assert.True(lexer.Tokens![0].Lexeme == literal);
    }

    [Theory]
    [InlineData("371.")]
    [InlineData("._61")]
    [InlineData("150__0")]
    //[InlineData("15_")]
    //[InlineData("23.45.67")]
    public void InvalidNumberLiteral (string literal) {
        Lexer lexer = SmallCompile(literal);
        Assert.True(lexer.Messages!.Errors.Count > 0);
        Assert.True(lexer.Messages!.Errors[0].Code == (int)MessageCode.UnexpectedCharacter);
    }

    private Lexer SmallCompile (string src) {
        Lexer lexer = new(src, new());
        lexer.Tokenize();
        return lexer;
    }
}
