using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET;

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
}
