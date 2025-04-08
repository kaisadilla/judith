use serde::Serialize;
use crate::SourceSpan;
use strum_macros::{EnumDiscriminants, EnumString, AsRefStr};
use crate::judith::lexical::token::Token;

#[derive(Debug, Serialize)]
pub enum MessageKind {
    Information,
    Warning,
    Error,
}

#[derive(Debug, Serialize)]
pub enum MessageOrigin {
    Lexer,
    Parser,
}

#[derive(Debug, Serialize)]
pub enum MessageSource {
    Span(SourceSpan),
    Token(Token),
}

impl MessageSource {
    fn get_line(&self) -> i64 {
        match self {
            MessageSource::Span(span) => span.line,
            MessageSource::Token(tok) => tok.base().line,
        }
    }
}

#[derive(Debug, Serialize)]
pub struct CompilerMessage {
    pub kind: MessageKind,
    pub origin: MessageOrigin,
    pub code: MessageCode,
    pub message: String,
    pub source: MessageSource,
}

impl CompilerMessage {
    pub fn to_string (&self) -> String {
        format!(
            "[{:?} / {:?}] {} - {:?} (Line {}",
            self.origin,
            self.kind,
            self.code.i32(),
            self.message,
            self.source.get_line()
        )
    }

    pub fn get_elaborate_message (&self, src: Option<&str>) -> String {
        let location = match &self.source {
            MessageSource::Span(span) => format!("line: {}", span.line),
            MessageSource::Token(tok) => format!("line: {}", tok.base().line), // TODO: More complex.
        };

        format!(
            "[{:?} / {:?}] {} - {:?}\n - at {}",
            self.origin,
            self.kind,
            self.code.i32(),
            self.message,
            location
        )
    }
}

#[derive(Debug, PartialEq, EnumDiscriminants, EnumString, AsRefStr, Serialize)]
#[strum_discriminants(derive(EnumString, AsRefStr))]
#[repr(i32)]
pub enum MessageCode {
    // 1xxx - Syntax-related errors
    UnexpectedCharacter{ character: char } = 1_000,
    InvalidNumber{ lexeme: String },
    UnterminatedString,

    // 2xxx - Parsing errors
    UnexpectedToken = 2_000,
    RightParenExpected,
    ExpressionExpected,
}

impl MessageCode {
    pub fn i32(&self) -> i32 {
        MessageCodeDiscriminants::from(self) as i32
    }
}

#[derive(Serialize)]
pub struct MessageContainer {
    pub infos: Vec<CompilerMessage>,
    pub warnings: Vec<CompilerMessage>,
    pub errors: Vec<CompilerMessage>,
}

impl MessageContainer {
    pub fn new() -> MessageContainer {
        MessageContainer {
            infos: vec![],
            warnings: vec![],
            errors: vec![],
        }
    }

    pub fn add (&mut self, msg: CompilerMessage) {
        match msg.kind {
            MessageKind::Information => self.infos.push(msg),
            MessageKind::Warning => self.warnings.push(msg),
            MessageKind::Error => self.errors.push(msg),
        };
    }

    pub fn add_all (&mut self, other: MessageContainer) {
        self.infos.extend(other.infos);
        self.warnings.extend(other.warnings);
        self.errors.extend(other.errors);
    }

    pub fn all_messages(&self) -> impl Iterator<Item = &CompilerMessage> {
        self.infos.iter().chain(self.warnings.iter()).chain(self.errors.iter())
    }

    pub fn count (&self) -> usize {
        self.infos.len() + self.warnings.len() + self.errors.len()
    }

    /// Prints all the messages in this container directly to console.
    pub fn dump_all (&self) {
        for m in self.all_messages() {
            println!("{}", m.get_elaborate_message(None));
        }
    }
}

pub struct Parser;

pub mod Lexer {
    use crate::judith::compiler_messages::{CompilerMessage, MessageCode, MessageSource};
    use crate::judith::compiler_messages::MessageKind::*;
    use crate::judith::compiler_messages::MessageOrigin::*;
    use crate::SourceSpan;

    pub fn unexpected_character(span: SourceSpan, unexpected_char: char) -> CompilerMessage {
        CompilerMessage {
            kind: Error,
            origin: Lexer,
            code: MessageCode::UnexpectedCharacter {character: unexpected_char},
            message: format!("Unexpected character: {}", unexpected_char),
            source: MessageSource::Span(span),
        }
    }

    pub fn invalid_number(span: SourceSpan, num: String) -> CompilerMessage {
        CompilerMessage {
            kind: Error,
            origin: Lexer,
            code: MessageCode::InvalidNumber {lexeme: num.clone()},
            message: format!("Invalid number: {}", num),
            source: MessageSource::Span(span),
        }
    }

    pub fn unterminated_string(span: SourceSpan) -> CompilerMessage {
        CompilerMessage {
            kind: Error,
            origin: Lexer,
            code: MessageCode::UnterminatedString,
            message: String::from("Unterminated string."),
            source: MessageSource::Span(span),
        }
    }
}

impl Parser {
    pub fn right_paren_expected(token: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::RightParenExpected,
            message: format!("Expected ')', found '{:?}'", token.kind()),
            source: MessageSource::Token(token),
        }
    }

    pub fn expression_expected(token: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::ExpressionExpected,
            message: format!("Expected expression, found '{:?}'", token.kind()),
            source: MessageSource::Token(token),
        }
    }
}