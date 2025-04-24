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
    IdentifierExpected,
    TypeAnnotationExpected,
    TypeExpected,
    LeftParenExpected,
    RightParenExpected,
    RightCurlyBracketExpected,
    RightSquareBracketExpected,
    ExpressionExpected,
    StatementExpected,
    BlockOpeningKeywordExpected,
    BodyExpected,
    ArrowExpected,
    ElsifBodyExpected,
    InExpected,
    DoExpected,
    EndExpected,
    ParameterExpected,
    ArgumentExpected,
    HidableItemExpected,
    VariableDeclaratorExpected,
    FieldInitializationExpected,
    InvalidTopLevelStatement,
    InvalidIntegerLiteral,
    InvalidFloatLiteral,
    ParameterTypeMustBeSpecified,
    FieldMustBeInitialized,
    ParameterTypeListExpected,
    ReturnTypeExpected,
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

pub struct Lexer;
pub struct Parser;

impl Lexer {
    pub fn unexpected_character(span: SourceSpan, unexpected_char: char) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Lexer,
            code: MessageCode::UnexpectedCharacter {character: unexpected_char},
            message: format!("Unexpected character: {}", unexpected_char),
            source: MessageSource::Span(span),
        }
    }

    pub fn invalid_number(span: SourceSpan, num: String) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Lexer,
            code: MessageCode::InvalidNumber {lexeme: num.clone()},
            message: format!("Invalid number: {}", num),
            source: MessageSource::Span(span),
        }
    }

    pub fn unterminated_string(span: SourceSpan) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Lexer,
            code: MessageCode::UnterminatedString,
            message: String::from("Unterminated string."),
            source: MessageSource::Span(span),
        }
    }
}

impl Parser {
    pub fn unexpected_token(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::UnexpectedToken,
            message: format!("Unexpected token: '{:?}'", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }

    pub fn identifier_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::IdentifierExpected,
            message: format!("Expected identifier, found '{:?}'.", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }

    pub fn type_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::TypeExpected,
            message: format!("Expected type, found '{:?}'.", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }

    pub fn right_paren_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::RightParenExpected,
            message: format!("Expected ')', found '{:?}'.", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }

    pub fn right_curly_bracket_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::RightCurlyBracketExpected,
            message: format!("Expected '}}', found '{:?}'.", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }

    pub fn right_square_bracket_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::RightSquareBracketExpected,
            message: format!("Expected ']', found '{:?}'.", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }

    pub fn expression_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::ExpressionExpected,
            message: format!("Expected expression, found '{:?}'.", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }

    pub fn body_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::BodyExpected,
            message: format!("Expected body, found '{:?}'.", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }

    pub fn elsif_body_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::ElsifBodyExpected,
            message: format!("Expected 'elsif' body, found '{:?}'.", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }

    pub fn end_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::EndExpected,
            message: format!("Expected end token, found '{:?}'.", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }

    pub fn argument_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::ArgumentExpected,
            message: format!("Expected argument, found '{:?}'.", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }

    pub fn variable_declarator_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::VariableDeclaratorExpected,
            message: format!(
                "Expected variable declarator (<name>, [<name>...] or {{ <name>... }}, found '{:?}'.",
                tok.kind()
            ),
            source: MessageSource::Token(tok),
        }
    }

    pub fn field_initialization_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::FieldInitializationExpected,
            message: format!("Expected field initialization, found '{:?}'.", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }

    pub fn field_must_be_initialized(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::FieldMustBeInitialized,
            message: String::from("Field must be initialized."),
            source: MessageSource::Token(tok),
        }
    }

    pub fn parameter_type_list_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::ParameterTypeListExpected,
            message: format!("Expected parameter type list (<type>...) found '{:?}'.", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }

    pub fn return_type_expected(tok: Token) -> CompilerMessage {
        CompilerMessage {
            kind: MessageKind::Error,
            origin: MessageOrigin::Parser,
            code: MessageCode::ReturnTypeExpected,
            message: format!("Expected return type (-> <type>), found '{:?}'.", tok.kind()),
            source: MessageSource::Token(tok),
        }
    }
}