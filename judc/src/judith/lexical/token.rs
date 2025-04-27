use std::fmt::{Debug, Formatter};
use serde::Serialize;
use crate::SourceSpan;

#[derive(Clone, Serialize)]
pub struct RegularToken {
    pub kind: TokenKind,
    pub lexeme: String,
    pub start: i64,
    pub end: i64,
    pub line: i64,
    pub leading_trivia: Vec<Trivia>,
    pub trailing_trivia: Vec<Trivia>,
}

#[derive(Clone, Serialize)]
pub struct StringToken {
    pub base: RegularToken,
    pub string_kind: StringLiteralKind,
    pub delimiter: char,
    pub delimiter_count: i32,
    pub column: i32,
}

#[derive(Debug, Clone, Serialize)]
pub enum StringLiteralKind {
    Regular,
    Raw,
}

#[derive(Debug, Clone, Serialize)]
pub enum Token {
    Regular(RegularToken),
    String(StringToken),
}

impl Token {
    pub fn kind(&self) -> TokenKind {
        match self {
            Token::Regular(t) => t.kind.clone(),
            Token::String(t) => t.base.kind.clone(),
        }
    }

    pub fn base(&self) -> &RegularToken {
        match self {
            Token::Regular(t) => t,
            Token::String(t) => &t.base,
        }
    }
}

#[derive(Debug, Clone, Serialize)]
pub struct Trivia {
    pub kind: TriviaKind,
    pub lexeme: String,
    pub span: SourceSpan,
}

#[derive(Debug, Copy, Clone, PartialEq, Serialize)]
pub enum TokenKind {
    // Error tokens
    Invalid = 0,

    // Single-character tokens
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
    QuestionMark,
    Ampersand,
    Pipe,

    // Two-character tokens.
    EqualEqual,
    BangEqual,
    TildeTilde,
    BangTilde,
    Less,
    LessEqual,
    Greater,
    GreaterEqual,
    EqualArrow,
    MinusArrow,
    DoubleColon,
    DoubleQuestionMark,

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
    KwOper,
    KwTypedef,
    KwStruct,
    KwInterface,
    KwClass,
    KwHid,
    KwPub,
    KwLet,
    KwFinal,
    KwMut,
    KwSh,
    KwRef,

    // Private keywords
    PkwPrint,

    // Directives

    // Other
    Comment,

    EOF,
}

impl TokenKind {
    pub fn name (&self) -> &str {
        match self {
            TokenKind::Invalid => "<invalid token>",
            TokenKind::Comma => "','",
            TokenKind::Colon => "'.'",
            TokenKind::LeftParen => "'('",
            TokenKind::RightParen => "')'",
            TokenKind::LeftCurlyBracket => "'{",
            TokenKind::RightCurlyBracket => "'}'",
            TokenKind::LeftSquareBracket => "'['",
            TokenKind::RightSquareBracket => "']'",
            TokenKind::LeftAngleBracket => "'<'",
            TokenKind::RightAngleBracket => "'>'",
            TokenKind::Plus => "'+'",
            TokenKind::Minus => "'-'",
            TokenKind::Asterisk => "'*'",
            TokenKind::Slash => "'/'",
            TokenKind::Equal => "'='",
            TokenKind::Bang => "'!'",
            TokenKind::Tilde => "'~'",
            TokenKind::Dot => "'.'",
            TokenKind::QuestionMark => "'?'",
            TokenKind::Ampersand => "'&'",
            TokenKind::Pipe => "'|'",
            TokenKind::EqualEqual => "'=='",
            TokenKind::BangEqual => "'!='",
            TokenKind::TildeTilde => "'~~'",
            TokenKind::BangTilde => "'!~'",
            TokenKind::Less => "'<'",
            TokenKind::LessEqual => "'<='",
            TokenKind::Greater => "'>'",
            TokenKind::GreaterEqual => "'>='",
            TokenKind::EqualArrow => "'=>'",
            TokenKind::MinusArrow => "'->'",
            TokenKind::DoubleColon => "'::'",
            TokenKind::DoubleQuestionMark => "'??'",
            TokenKind::EqualEqualEqual => "'==='",
            TokenKind::BangEqualEqual => "'!=='",
            TokenKind::Identifier => "<identifier>",
            TokenKind::String => "<string literal>",
            TokenKind::Number => "<number literal>",
            TokenKind::KwTrue => "'true'",
            TokenKind::KwFalse => "'false'",
            TokenKind::KwNull => "'null'",
            TokenKind::KwUndefined => "'undefined'",
            TokenKind::KwNot => "'not'",
            TokenKind::KwAnd => "'and'",
            TokenKind::KwOr => "'or'",
            TokenKind::KwEnd => "'end'",
            TokenKind::KwIf => "'if'",
            TokenKind::KwElse => "'else'",
            TokenKind::KwElsif => "'elsif'",
            TokenKind::KwMatch => "'match'",
            TokenKind::KwThen => "'then'",
            TokenKind::KwLoop => "'loop'",
            TokenKind::KwWhile => "'while'",
            TokenKind::KwFor => "'for'",
            TokenKind::KwIn => "'in'",
            TokenKind::KwDo => "'do'",
            TokenKind::KwReturn => "'return'",
            TokenKind::KwYield => "'yield'",
            TokenKind::KwBreak => "'break'",
            TokenKind::KwContinue => "'continue'",
            TokenKind::KwGoto => "'goto'",
            TokenKind::KwFunc => "'func'",
            TokenKind::KwGenerator => "'generator'",
            TokenKind::KwOper => "'oper'",
            TokenKind::KwTypedef => "'typedef'",
            TokenKind::KwStruct => "'struct'",
            TokenKind::KwInterface => "'interface'",
            TokenKind::KwClass => "'class'",
            TokenKind::KwHid => "'hid'",
            TokenKind::KwPub => "'pub'",
            TokenKind::KwLet => "'let'",
            TokenKind::KwFinal => "'final'",
            TokenKind::KwMut => "'mut'",
            TokenKind::KwSh => "'sh'",
            TokenKind::KwRef => "'ref'",
            TokenKind::PkwPrint => "'__p_print'",
            TokenKind::Comment => "<comment>",
            TokenKind::EOF => "<end of file>",
        }
    }
}

#[derive(Debug, Clone, PartialEq, Serialize)]
pub enum TriviaKind {
    SingleLineComment,
    MultiLineComment,
    Whitespace,
    LineBreak,
    Directive,
}

impl Debug for RegularToken {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("RegularToken")
            .field("kind", &self.kind)
            .field("lexeme", &self.lexeme)
            .field("start", &self.start)
            .field("end", &self.end)
            .field("line", &self.line)
            .field("leading_trivia", &self.leading_trivia.len())
            .field("trailing_trivia", &self.trailing_trivia.len())
            .finish()
    }
}

impl Debug for StringToken {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("StringToken")
            .field("base", &self.base)
            .field("string_kind", &self.string_kind)
            .field("delimiter", &self.delimiter)
            .field("delimiter_count", &self.delimiter_count)
            .field("column", &self.column)
            .finish()
    }
}