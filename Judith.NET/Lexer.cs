using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Judith.NET;

public class Lexer {
    private static readonly Dictionary<string, TokenKind> KEYWORDS = new() {
        ["and"] = TokenKind.KwAnd,
        ["break"] = TokenKind.KwBreak,
        ["const"] = TokenKind.KwConst,
        ["continue"] = TokenKind.KwContinue,
        ["do"] = TokenKind.KwDo,
        ["else"] = TokenKind.KwElse,
        ["elsif"] = TokenKind.KwElsif,
        ["end"] = TokenKind.KwEnd,
        ["false"] = TokenKind.KwFalse,
        ["for"] = TokenKind.KwFor,
        ["func"] = TokenKind.KwFunc,
        ["generator"] = TokenKind.KwGenerator,
        ["goto"] = TokenKind.KwGoto,
        ["hid"] = TokenKind.KwHid,
        ["if"] = TokenKind.KwIf,
        ["in"] = TokenKind.KwIn,
        ["loop"] = TokenKind.KwLoop,
        ["match"] = TokenKind.KwMatch,
        ["not"] = TokenKind.KwNot,
        ["null"] = TokenKind.KwNull,
        ["or"] = TokenKind.KwOr,
        ["return"] = TokenKind.KwReturn,
        ["then"] = TokenKind.KwThen,
        ["true"] = TokenKind.KwTrue,
        ["undefined"] = TokenKind.KwUndefined,
        ["var"] = TokenKind.KwVar,
        ["while"] = TokenKind.KwWhile,
        ["yield"] = TokenKind.KwYield,
        ["__p_print"] = TokenKind.PkwPrint,
    };

    public MessageContainer Messages { get; private set; } = new();

    private readonly string _src;

    /// <summary>
    /// The first character of the token being scanned.
    /// </summary>
    private int _tokenStart = 0;
    /// <summary>
    /// The current character being scanned.
    /// </summary>
    private int _cursor = 0;
    /// <summary>
    /// The line in the source string the cursor is currently in.
    /// </summary>
    private int _line = 1;
    /// <summary>
    /// The position of the current character within the current line.
    /// </summary>
    private int _column = 0;

    /// <summary>
    /// True if this lexer's source contains any errors.
    /// </summary>
    public bool HasError { get; private set; } = false;
    /// <summary>
    /// The tokens in this lexer's source.
    /// </summary>
    public List<Token>? Tokens { get; private set; } = null;

    public Lexer (string src) {
        _src = src;
    }

    [MemberNotNull(nameof(Tokens))]
    public void Tokenize () {
        Tokens = new();

        while (Tokens.Count == 0 || Tokens[^1].Kind != TokenKind.EOF) {
            Token? token = Next();
            if (token == null) continue;
            
            Tokens.Add(token);
        }
    }

    private Token? Next () {
        SkipWhitespaces();

        _tokenStart = _cursor;

        if (IsAtEnd()) return MakeToken(TokenKind.EOF);
        char c = Advance();

        switch (c) {
            case ',':
                return MakeToken(TokenKind.Comma);
            case ':':
                if (Match(':')) {
                    return MakeToken(TokenKind.DoubleColon);
                }

                return MakeToken(TokenKind.Colon);
            case '(':
                return MakeToken(TokenKind.LeftParen);
            case ')':
                return MakeToken(TokenKind.RightParen);
            case '{':
                return MakeToken(TokenKind.LeftCurlyBracket);
            case '}':
                return MakeToken(TokenKind.RightCurlyBracket);
            case '[':
                return MakeToken(TokenKind.LeftSquareBracket);
            case ']':
                return MakeToken(TokenKind.RightSquareBracket);
            case '+':
                return MakeToken(TokenKind.Plus);
            case '-':
                // Token '--' for comment.
                if (Match('-')) {
                    return ScanSingleLineComment();
                }
                // Negative number ('-' followed by number leading char).
                else if (IsNumberLeadingChar(Peek())) {
                    return ScanNumber(Advance());
                }

                return MakeToken(TokenKind.Minus);
            case '*':
                return MakeToken(TokenKind.Asterisk);
            case '/':
                return MakeToken(TokenKind.Slash);
            case '=':
                if (Match('=')) {
                    if (Match('=')) {
                        return MakeToken(TokenKind.EqualEqualEqual);
                    }

                    return MakeToken(TokenKind.EqualEqual);
                }
                if (Match('>')) {
                    return MakeToken(TokenKind.EqualArrow);
                }

                return MakeToken(TokenKind.Equal);
            case '!':
                if (Match('=')) {
                    if (Match('=')) {
                        return MakeToken(TokenKind.BangEqualEqual);
                    }

                    return MakeToken(TokenKind.BangEqual);
                }

                return MakeToken(TokenKind.Bang);
            case '~':
                if (Match('=')) {
                    return MakeToken(TokenKind.TildeEqual);
                }

                return MakeToken(TokenKind.Tilde);
            case '.':
                return MakeToken(TokenKind.Dot);
            case '<':
                if (Match('=')) {
                    return MakeToken(TokenKind.LessEqual);
                }

                return MakeToken(TokenKind.Less);
            case '>':
                if (Match('=')) {
                    return MakeToken(TokenKind.GreaterEqual);
                }

                return MakeToken(TokenKind.Greater);
            case '"':
                return ScanString('"', _column - 1);
            case '`':
                return ScanString('`', _column - 1);
        }

        // Positive number (token that starts with number leading char).
        if (IsNumberLeadingChar(c)) {
            // If the first character is ".", then the next character has to
            // be a digit. If it's not, then the first character is a digit and
            // ScanNumber() takes care of the rest.
            if (c != '.' || IsDigit(Peek())) {
                return ScanNumber(c);
            }
        }
        else if (IsIdentifierLeadingChar(c)) {
            return ScanIdentifierOrString();
        }

        // Couldn't match this character to anything, so it's an error.
        Error(CompilerMessage.Lexer.UnexpectedCharacter(_line, c));
        return null;
    }

    #region Check characters
    /// <summary>
    /// Returns true if the cursor is at or beyond the end of the source's string.
    /// </summary>
    private bool IsAtEnd () {
        return _cursor >= _src.Length;
    }

    /// <summary>
    /// Returns the character the cursor is at and moves the cursor forward.
    /// </summary>
    /// <returns></returns>
    private char Advance () {
        _cursor++;
        _column++;

        if (IsAtEnd() == false && _src[_cursor] == '\n') {
            _line++;
            _column = 0;
        }

        return _src[_cursor - 1];
    }

    /// <summary>
    /// Checks the character the cursor is at. If it matches the character
    /// given, moves the cursor forward. Returns true when the character given
    /// was successfully matched.
    /// </summary>
    /// <param name="expected">The character to match.</param>
    private bool Match (char expected) {
        if (IsAtEnd()) return false;
        if (_src[_cursor] != expected) return false;

        Advance();
        return true;
    }

    /// <summary>
    /// Returns the character the cursor is at, without moving the cursor.
    /// </summary>
    private char Peek () {
        if (IsAtEnd()) return '\0';

        return _src[_cursor];
    }

    /// <summary>
    /// Returns the next character after the cursor, without moving the cursor.
    /// </summary>
    private char PeekNext () {
        if (_cursor + 1 >= _src.Length) return '\0';

        return _src[_cursor + 1];
    }

    /// <summary>
    /// Extracts a lexeme from the source code, given its start and end indices.
    /// </summary>
    /// <param name="start">The first character in the lexeme.</param>
    /// <param name="end">The first character not in the lexeme.</param>
    /// <returns></returns>
    private string ExtractLexeme (int start, int end) {
        return _src.Substring(start, end - start);
    }
    #endregion

    #region Test characters
    /// <summary>
    /// Returns true if the character given is used for spaces or new lines.
    /// </summary>
    public bool IsWhitespace (char c) {
        return c == ' ' || c == '\r' || c == '\n' || c == '\t';
    }

    /// <summary>
    /// Returns true if the character given is a digit.
    /// </summary>
    public bool IsDigit (char c) {
        return c >= '0' && c <= '9';
    }

    /// <summary>
    /// Returns true if the character is an ASCII letter (A-Z, uppercase or
    /// lowercase).
    /// </summary>
    public bool IsLetter (char c) {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }

    /// <summary>
    /// Returns true if the character given can be the first character in a
    /// numeric literal. This excludes the minus sign.
    /// </summary>
    public bool IsNumberLeadingChar (char c) {
        return IsDigit(c) || c == '.';
    }

    /// <summary>
    /// Returns true if the character given can be the first character in an
    /// identifier. This includes the identifier escape character '\'.
    /// </summary>
    public bool IsIdentifierLeadingChar (char c) {
        return IsLetter(c) || c == '_' || c == '\\';
    }

    /// <summary>
    /// Returns true if the character given can be part of an identifier,
    /// excluding the leading character in it. This is letters, numbers and
    /// underscores.
    /// </summary>
    public bool IsIdentifierChar (char c) {
        return IsLetter(c) || c == '_' || IsDigit(c);
    }
    #endregion

    /// <summary>
    /// Moves the cursor to the next non-whitespace character in the source.
    /// </summary>
    private void SkipWhitespaces () {
        char c;
        while (IsWhitespace(c = Peek())) {
            Advance();
        }
    }

    #region Scan functions
    /// <summary>
    /// Scans a single-line comment. This method assumes that the token that
    /// starts the comment (e.g. '--') has been consumed already.
    /// </summary>
    private Token? ScanSingleLineComment () {
        while (Peek() != '\r' && Peek() != '\n' && IsAtEnd() == false) {
            Advance();
        }
        
        return MakeToken(TokenKind.Comment);
    }

    /// <summary>
    /// Scans a numeric literal. This function assumes that the first character
    /// in the literal has already been consumed.
    /// </summary>
    /// <param name="firstChar">The first character in the literal.</param>
    private Token? ScanNumber (char firstChar) {
        // The amount of dot characters in this literal.
        int dotsFound = firstChar == '.' ? 1 : 0;

        // Whether the next character in this literal can be an underscore.
        // Numeric literals can include underscores as long as the underscore
        // doesn't follow a decimal point or another underscore; or is the last
        // character in the lexeme.
        bool underscoreAllowed = IsDigit(firstChar);

        char c = Peek();
        while (true) {
            if (c == '.') {
                // If this isn't the first decimal point we find, the number
                // ends before it (this isn't necessarily an error, as Judith
                // allows fields to be called on numbers (e.g. 7.str())
                if (dotsFound > 0) {
                    break;
                }

                char c2 = PeekNext();
                bool numberContinues = IsDigit(c2);

                // If the character after the dot is not valid for a number,
                // the number ends before it. Again, this isn't necessarily an
                // error.
                if (numberContinues == false) {
                    break;
                }

                // The number continues, increase the number of dots inside it.
                dotsFound++;
            }
            else if (c == '_') {
                // If an underscore is not allowed, the number ends.
                if (underscoreAllowed == false) {
                    break;
                }

                // Can't chain two underscores together, so next character
                // cannot be an underscore.
                underscoreAllowed = false;
            }
            // We found something that is not a digit, dot or underscore.
            else if (IsDigit(c) == false) {
                // This is never allowed.
                break;
            }
            // c is a regular digit.
            else {
                // Underscore will be allowed after it.
                underscoreAllowed = true;
            }

            Advance();
            c = Peek();
        }

        int numberEndIndex = _cursor;

        return MakeToken(TokenKind.Number);
    }

    public Token? ScanIdentifierOrString () {
        int startColumn = _column - 1;

        // We can be scanning an identifier (including keywords) or a string
        // literal led by flags (e.g. ef"my string").
        while (IsIdentifierChar(Peek()) || Peek() == '"' || Peek() == '`') {
            // If we encounter a string delimiter character, this is a string
            // with leading flags.
            if (Match('"')) return ScanString('"', startColumn);
            if (Match('`')) return ScanString('`', startColumn);

            Advance();
        }

        string lexeme = ExtractLexeme(_tokenStart, _cursor);
        var kind = KEYWORDS.GetValueOrDefault(lexeme, TokenKind.Identifier);

        return MakeToken(kind);
    }

    /// <summary>
    /// Scans a string token until its end. This method assumes that everything
    /// up to the FIRST quote in the string has been consumed already. For
    /// example, for the token |"test"|, only " has been already consumed. For
    /// a string with flags, like |f"test"|, f" has already been consumed. For
    /// a string with multiple delimiting quotes, like |ff"""test"""|, only the
    /// first delimiting quote (and characters before it) have been consumed:
    /// ff" (""test""" hasn't been consumed yet).
    /// </summary>
    /// <param name="quotingChar">The character used as delimiter.</param>
    /// <param name="startColumn">The column the string started at.</param>
    /// <returns></returns>
    public Token? ScanString (char quotingChar, int startColumn) {
        int openingQuotes = 1; // the one that triggered this call.
        while (Match(quotingChar)) openingQuotes++;

        // empty string: "". Note that two quotes cannot delimit a string (e.g.
        // ""test"" is not possible, as it would be parsed as [""][test][""].
        if (openingQuotes == 2) {
            return MakeStringToken(quotingChar, 1, startColumn);
        }

        // the string will only end when it encounters as many quoting chars in
        // a row as those used to start the string.
        int closingQuotes = 0;
        while (closingQuotes < openingQuotes && IsAtEnd() == false) {
            char c = Advance();

            if (c == quotingChar) closingQuotes++;
            else closingQuotes = 0; // If a non-quote char is found, the counter is reset.
        }

        if (IsAtEnd()) {
            Error(CompilerMessage.Lexer.UnterminatedString(_line));
            return null;
        }

        return MakeStringToken(quotingChar, openingQuotes, startColumn);
    }
    #endregion

    #region Token factory
    private Token MakeToken (TokenKind kind) {
        string lexeme = ExtractLexeme(_tokenStart, _cursor);

        return new Token(kind, lexeme, _tokenStart, _cursor, _line);
    }

    private StringToken MakeStringToken (char quotingChar, int quoteCount, int columnIndex) {
        string lexeme = ExtractLexeme(_tokenStart, _cursor);
        var stringKind = quoteCount switch {
            1 => StringLiteralKind.Regular,
            _ => StringLiteralKind.Raw,
        };

        return new StringToken(
            lexeme: lexeme,
            start: _tokenStart,
            end: _cursor,
            line: _line,
            stringKind: stringKind,
            delimiter: quotingChar,
            delimiterCount: quoteCount,
            columnIndex: columnIndex
        );
    }
    #endregion

    private void Error (CompilerMessage message) {
        HasError = true;
        Messages.Add(message);
    }
}
