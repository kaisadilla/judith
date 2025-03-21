using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;

namespace Judith.NET.analysis.lexical;

public class Token {
    /// <summary>
    /// This token's type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public TokenKind Kind { get; init; }
    /// <summary>
    /// The string of text in the source file that was parsed as this token.
    /// </summary>
    public string Lexeme { get; init; }
    /// <summary>
    /// The position of the first character that is part of this token.
    /// </summary>
    public int Start { get; init; }
    /// <summary>
    /// The position of the first character that is not part of this token.
    /// </summary>
    public int End { get; init; }
    /// <summary>
    /// The line this token is at. If this token spans multiple lines, this
    /// number indicates the line the first character is at.
    /// </summary>
    public int Line { get; init; }
    /// <summary>
    /// The pieces of trivia that appear before this token.
    /// </summary>
    public List<Trivia> LeadingTrivia { get; init; } = [];
    /// <summary>
    /// The pieces of trivia that appear after this token.
    /// </summary>
    public List<Trivia> TrailingTrivia { get; init; } = [];

    public Token (TokenKind kind, string lexeme, int start, int end, int line) {
        Kind = kind;
        Lexeme = lexeme;
        Start = start;
        End = end;
        Line = line;
    }

    public override string ToString () {
        return $"{{{Kind}, '{Lexeme}'}}";
    }

    public static string GetTokenName (TokenKind kind) {
        return kind switch {
            TokenKind.Invalid => "Invalid",
            // Single-character tokens.
            TokenKind.Comma => "Comma",
            TokenKind.Colon => "Colon",
            TokenKind.LeftParen => "Opening parenthesis",
            TokenKind.RightParen => "Closing parenthesis",
            TokenKind.LeftCurlyBracket => "Opening curly bracket",
            TokenKind.RightCurlyBracket => "Closing curly bracket",
            TokenKind.LeftSquareBracket => "Opening square bracket",
            TokenKind.RightSquareBracket => "Closing square bracket",
            TokenKind.LeftAngleBracket => "Opening angle bracket",
            TokenKind.RightAngleBracket => "Closing angle bracket",
            TokenKind.Plus => "Plus sign",
            TokenKind.Minus => "Minus sign",
            TokenKind.Asterisk => "Asterisk",
            TokenKind.Slash => "Slash",
            TokenKind.Equal => "Equal sign",
            TokenKind.Bang => "Exclamation mark",
            TokenKind.Tilde => "Tilde",
            TokenKind.Dot => "Dot",
            TokenKind.QuestionMark => "Question mark",
            TokenKind.Pipe => "Pipe",

            // Two-character tokens.
            TokenKind.EqualEqual => "Equal equal sign",
            TokenKind.BangEqual => "Exclamation mark equal sign",
            TokenKind.TildeEqual => "Tilde equal sign",
            TokenKind.Less => "Less sign",
            TokenKind.LessEqual => "Less or equal sign",
            TokenKind.Greater => "Greater sign",
            TokenKind.GreaterEqual => "Greater or equal sign",
            TokenKind.EqualArrow => "Equal arrow",
            TokenKind.MinusArrow => "Minus arrow",
            TokenKind.DoubleColon => "Double colon",
            TokenKind.DoubleQuestionMark => "Double question mark",

            // Three-character tokens.
            TokenKind.EqualEqualEqual => "Triple equal sign",
            TokenKind.BangEqualEqual => "Not triple equal sign",

            // % operators

            // Literals
            TokenKind.Identifier => "Identifier",
            TokenKind.String => "String",
            TokenKind.Number => "Number",

            // Keywords
            TokenKind.KwConst => "const",
            TokenKind.KwVar => "var",
            TokenKind.KwTrue => "true",
            TokenKind.KwFalse => "false",
            TokenKind.KwNull => "null",
            TokenKind.KwUndefined => "undefined",
            TokenKind.KwNot => "not",
            TokenKind.KwAnd => "and",
            TokenKind.KwOr => "or",
            TokenKind.KwEnd => "end",
            TokenKind.KwIf => "if",
            TokenKind.KwElse => "else",
            TokenKind.KwElsif => "elsif",
            TokenKind.KwThen => "then",
            TokenKind.KwMatch => "match",
            TokenKind.KwLoop => "loop",
            TokenKind.KwWhile => "while",
            TokenKind.KwFor => "for",
            TokenKind.KwIn => "in",
            TokenKind.KwDo => "do",
            TokenKind.KwReturn => "return",
            TokenKind.KwYield => "yield",
            TokenKind.KwBreak => "break",
            TokenKind.KwContinue => "continue",
            TokenKind.KwGoto => "goto",
            TokenKind.KwFunc => "func",
            TokenKind.KwGenerator => "generator",
            TokenKind.KwHid => "hid",
            TokenKind.KwOper => "oper",
            TokenKind.KwTypedef => "typedef",
            TokenKind.KwStruct => "struct",
            TokenKind.KwInterface => "interface",
            TokenKind.KwClass => "class",
            TokenKind.KwPub => "pub",
            TokenKind.KwMut => "mut",
            TokenKind.KwStatic => "static",

            // Private keywords
            TokenKind.PkwPrint => "__p_print",

            // Directives

            // Other
            TokenKind.Comment => "Comment",

            TokenKind.EOF => "End of file",
            TokenKind.Error => "Error",
            _ => "Invalid token code"
        };
    }
}

public class StringToken : Token {
    public StringLiteralKind StringKind { get; init; }
    /// <summary>
    /// The character used to delimit the string.
    /// </summary>
    public char Delimiter { get; init; }
    /// <summary>
    /// The amount of characaters used to delimit the string.
    /// </summary>
    public int DelimiterCount { get; init; }
    /// <summary>
    /// The index of the first character of this string in the column it
    /// belongs to.
    /// </summary>
    public int ColumnIndex { get; init; }

    public StringToken (
        string lexeme,
        int start,
        int end,
        int line,
        StringLiteralKind stringKind,
        char delimiter,
        int delimiterCount,
        int columnIndex
    )
        : base(TokenKind.String, lexeme, start, end, line) {
        StringKind = stringKind;
        Delimiter = delimiter;
        DelimiterCount = delimiterCount;
        ColumnIndex = columnIndex;
    }

    public override string ToString () {
        return $"{{{Kind}, '{Lexeme}', Kind: {StringKind}, Delimiter: " +
            $"'{Delimiter}' (x{DelimiterCount})}}";
    }
}