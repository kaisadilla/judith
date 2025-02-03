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

    // Two-character tokens.

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

    // Directives

    // Other
    Comment,

    EOF,
    Error,
}
