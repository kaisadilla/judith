﻿using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Judith.NET;

[JsonConverter(typeof(StringEnumConverter))]
public enum TokenKind {
    // Single-character tokens.
    Comma,
    Colon,
    LeftParen,
    RightParen,
    LeftCurlyBracket,
    RightCurlyBracket,
    LeftSquareBracket,
    RightSquareBracket,
    LeftAngleBracket,
    RightAngleBracket,
    Plus,
    Minus,
    Asterisk,
    Slash,
    Equal,
    Bang,
    Tilde,
    Dot,

    // Two-character tokens.
    EqualEqual,
    BangEqual,
    TildeEqual,
    Less,
    LessEqual,
    Greater,
    GreaterEqual,
    EqualArrow,
    DoubleColon,

    // Three-character tokens.
    EqualEqualEqual,
    BangEqualEqual,

    // % operators

    // Identifier
    Identifier,

    // Literals
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
    KwReturn,
    KwYield,
    KwBreak,
    KwContinue,
    KwGoto,
    KwFunc,
    KwGenerator,
    KwTypedef,
    KwStruct,
    KwInterface,
    KwClass,
    KwHid,
    KwPub,
    KwMut,
    KwStatic,

    // Private keywords
    PkwPrint,

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
