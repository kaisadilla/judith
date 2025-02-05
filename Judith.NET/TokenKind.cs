using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET;

public enum TokenKind {
    // Single-character tokens.
    Comma,
    Colon,
    LeftParen,
    RightParen,
    Plus,
    Minus,
    Asterisk,
    Slash,
    Equal,
    Bang,

    // Two-character tokens.
    EqualEqual,
    BangEqual,
    Less,
    LessEqual,
    Greater,
    GreaterEqual,
    EqualArrow,

    // Three-character tokens.

    // % operators

    // Literals
    Identifier,
    String,
    Number,

    // Keywords
    KwConst,
    KwVar,
    KwTrue,
    KwFalse,
    KwNull,
    KwUndefined,
    KwNot,
    KwAnd,
    KwOr,
    KwEnd,
    KwIf,
    KwElse,
    KwElsif,
    KwMatch,
    KwThen,
    KwLoop,
    KwWhile,
    KwFor,
    KwIn,
    KwDo,

    // Directives

    // Other
    Comment,

    EOF,
    Error,
}

public enum StringLiteralKind {
    Regular,
    Raw,
}
