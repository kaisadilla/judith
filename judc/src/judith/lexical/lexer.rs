use std::collections::HashMap;
use std::iter::{Enumerate, Peekable};
use std::str::Chars;
use once_cell::sync::Lazy;
use crate::judith::compiler_messages;
use crate::judith::compiler_messages::{CompilerMessage, MessageContainer, MessageOrigin};
use crate::judith::lexical::token::{RegularToken, StringLiteralKind, StringToken, Token, TokenKind, Trivia, TriviaKind};
use crate::SourceSpan;

const FIRST_LINE: usize = 1;
const FIRST_COLUMN: usize = 1;

/// Maps every string that represents a keyword to its keyword token kind.
static KEYWORD_MAP: Lazy<KeywordMap> = Lazy::new(KeywordMap::new);

struct KeywordMap {
    map: HashMap<&'static str, TokenKind>,
}

impl KeywordMap {
    fn new() -> Self {
        let mut map = HashMap::new();
        map.insert("and", TokenKind::KwAnd);
        map.insert("break", TokenKind::KwBreak);
        map.insert("class", TokenKind::KwClass);
        map.insert("const", TokenKind::KwConst);
        map.insert("continue", TokenKind::KwContinue);
        map.insert("do", TokenKind::KwDo);
        map.insert("else", TokenKind::KwElse);
        map.insert("elsif", TokenKind::KwElsif);
        map.insert("end", TokenKind::KwEnd);
        map.insert("false", TokenKind::KwFalse);
        map.insert("for", TokenKind::KwFor);
        map.insert("func", TokenKind::KwFunc);
        map.insert("generator", TokenKind::KwGenerator);
        map.insert("goto", TokenKind::KwGoto);
        map.insert("hid", TokenKind::KwHid);
        map.insert("if", TokenKind::KwIf);
        map.insert("in", TokenKind::KwIn);
        map.insert("interface", TokenKind::KwInterface);
        map.insert("loop", TokenKind::KwLoop);
        map.insert("match", TokenKind::KwMatch);
        map.insert("not", TokenKind::KwNot);
        map.insert("or", TokenKind::KwOr);
        map.insert("pub", TokenKind::KwPub);
        map.insert("return", TokenKind::KwReturn);
        map.insert("struct", TokenKind::KwStruct);
        map.insert("then", TokenKind::KwThen);
        map.insert("true", TokenKind::KwTrue);
        map.insert("typedef", TokenKind::KwTypedef);
        map.insert("undefined", TokenKind::KwUndefined);
        map.insert("var", TokenKind::KwVar);
        map.insert("while", TokenKind::KwWhile);
        map.insert("yield", TokenKind::KwYield);
        map.insert("__p_print", TokenKind::PkwPrint);

        KeywordMap { map }
    }

    fn get(&self, str: &str) -> Option<TokenKind> {
        self.map.get(str).cloned()
    }
}

pub struct Lexer<'a> {
    src: &'a str,
    chars: Peekable<Enumerate<Chars<'a>>>,
    start: usize,
    line: usize,
    column: usize,
    hasErrors: bool,
    messages: MessageContainer,
}

pub struct LexerResult {
    pub tokens: Vec<Token>,
    pub messages: MessageContainer,
}

impl<'a> Lexer<'a> {
    pub fn new(src: &'a str) -> Self {
        Self {
            src,
            chars: src.chars().enumerate().peekable(),
            start: 0,
            line: FIRST_LINE,
            column: FIRST_COLUMN,
            hasErrors: false,
            messages: MessageContainer::new(),
        }
    }

    pub fn next_token(&mut self) -> Token {
        let leading_trivia = self.consume_leading_trivia();
        let mut token = self.consume_token();
        let trailing_trivia = self.consume_trailing_trivia();

        match &mut token {
            Token::Regular(t) => {
                t.leading_trivia = leading_trivia;
                t.trailing_trivia = trailing_trivia;
            },
            Token::String(t) => {
                t.base.leading_trivia = leading_trivia;
                t.base.trailing_trivia = trailing_trivia;
            }
        }

        token
    }

    fn consume_leading_trivia(&mut self) -> Vec<Trivia> {
        let mut trivia: Vec<Trivia> = Vec::new();

        while let Some(t) = self.next_trivia() {
            trivia.push(t);
        }

        trivia
    }

    fn consume_trailing_trivia(&mut self) -> Vec<Trivia> {
        let mut trivia: Vec<Trivia> = Vec::new();

        while let Some(t) = self.next_trivia() {
            let trivia_kind = t.kind.clone();
            trivia.push(t);

            // When we encounter a line break trivia, any subsequent trivia becomes leading trivia
            // for the next token.
            if trivia_kind == TriviaKind::LineBreak {
                break;
            }

            // If the next character is the starting character of a directive trivia (or if there's
            // no next character), then we don't consume any more trivia.
            match self.peek() {
                None => break,
                Some(c) if c == '#' => break,
                _ => {}
            }
        }

        trivia
    }

    fn consume_token(&mut self) -> Token {
        self.start = self.cursor();

        if self.is_at_end() {
            return self.make_token(TokenKind::EOF);
        }
        let c = self.advance().unwrap(); // if is_at_end is false, then there has to be a char.

        match c {
            ',' => self.make_token(TokenKind::Comma),
            ':' => match self.try_match(':') {
                true => self.make_token(TokenKind::DoubleColon), // ::
                false => self.make_token(TokenKind::Colon), // :
            },
            '(' => self.make_token(TokenKind::LeftParen), // (
            ')' => self.make_token(TokenKind::RightParen), // )
            '{' => self.make_token(TokenKind::LeftCurlyBracket), // {
            '}' => self.make_token(TokenKind::RightCurlyBracket), // }
            '[' => self.make_token(TokenKind::LeftSquareBracket), // [
            ']' => self.make_token(TokenKind::RightSquareBracket), // ]
            '+' => self.make_token(TokenKind::Plus), // +
            '-' => {
                if self.try_match('-') {
                    panic!("Comments should've been caught as trivia!");
                }
                if self.try_match('>') {
                    return self.make_token(TokenKind::MinusArrow) // ->
                }

                let c = self.peek();
                if c != None && Self::is_number_leading_char(c.unwrap()) {
                    self.advance();
                    return self.scan_number(c.unwrap()); // number starting with '-'.
                }

                self.make_token(TokenKind::Minus) // -
            },
            '*' => self.make_token(TokenKind::Asterisk),
            '/' => self.make_token(TokenKind::Slash),
            '=' => match self.try_match('=') {
                true => match self.try_match('=') {
                    true => self.make_token(TokenKind::EqualEqualEqual), // ===
                    false => self.make_token(TokenKind::EqualEqual), // ==
                },
                false => match self.try_match('>') {
                    true => self.make_token(TokenKind::EqualArrow), // =>
                    false => self.make_token(TokenKind::Equal), // =
                }
            }
            '!' => match self.try_match('=') {
                true => match self.try_match('=') {
                    true => self.make_token(TokenKind::BangEqualEqual), // !==
                    false => self.make_token(TokenKind::BangEqual), // !=
                },
                false => self.make_token(TokenKind::Bang), // !
            },
            '?' => match self.try_match('?') {
                true => self.make_token(TokenKind::DoubleQuestionMark), // ??
                false => self.make_token(TokenKind::QuestionMark), // ?
            },
            '~' => match self.try_match('=') {
                true => self.make_token(TokenKind::TildeEqual),
                false => self.make_token(TokenKind::Tilde),
            },
            '.' => {
                // When we encounter ".", we may be encountering the dot token, or a numeric literal
                // in the form of ".0000".
                let c2 = self.peek();
                if c2.is_none() == false && Self::is_digit(c2.unwrap()) {
                    return self.scan_number(c);
                }

                self.make_token(TokenKind::Dot)
            }
            '<' => match self.try_match('=') {
                true => self.make_token(TokenKind::LessEqual),
                false => self.make_token(TokenKind::Less),
            },
            '>' => match self.try_match('=') {
                true => self.make_token(TokenKind::GreaterEqual),
                false => self.make_token(TokenKind::Greater),
            },
            '|' => self.make_token(TokenKind::Pipe),
            '"' => self.scan_string('"', self.column - 1),
            '`' => self.scan_string('`', self.column - 1),
            _ if Self::is_number_leading_char(c) => {
                self.scan_number(c) // Here we know c is not '.', as that case is already tested.
            },
            _ if Self::is_identifier_leading_char(c) => {
                self.scan_literal_like() // this includes literals, keywords and strings with prefixes.
            },
            _ => {
                let cursor = self.cursor() as i64;
                self.error(compiler_messages::Lexer::unexpected_character(SourceSpan {
                    start: cursor,
                    end: cursor,
                    line: self.line as i64,
                }, c));
                self.make_token(TokenKind::Invalid)
            }
        }
    }

    fn next_trivia(&mut self) -> Option<Trivia> {
        self.start = self.cursor();
        let char = self.peek();

        if let None = char {
            return None;
        }
        let char = char.unwrap();

        // Consume whitespace trivia.
        if Self::is_whitespace(char) {
            return Some(self.scan_whitespace_trivia());
        }
        // Consume new line trivia.
        else if Self::is_newline(char) {
            self.advance(); // consume the newline sequence.
            return Some(self.make_trivia(TriviaKind::LineBreak));
        }
        // Consume directive trivia.
        else if char == '#' {
            todo!("Directives.");
        }
        // Consume comment trivia (single line or multiline)
        else if char == '-' {
            // Check if it's followed by another "-" to start the comment.
            let char2 = self.peek_next();
            if char2.is_none() {
                return None;
            }

            if char2 != Some('-') {
                return None;
            }

            // Advance the "--" characters.
            self.advance();
            self.advance();

            return match self.peek() {
                None => Some(self.make_trivia(TriviaKind::SingleLineComment)),
                Some(char) if char == '!' => Some(self.scan_multiline_comment()),
                Some(_) => Some(self.scan_single_line_comment()),
            }
        }

        None
    }

    /// Scans a numeric literal. This function assumes that the first character in the literal has
    /// already been consumed, and is being passed as the first parameter.
    /// * `first` The first character of this number, which is already consumed.
    fn scan_number(&mut self, first: char) -> Token {
        // Whether we've already found a "." character in this literal.
        let mut dot_found = first == '.';
        // Whether we've already found the "e" character in this literal, used for scientific
        // notation (e.g. 1.81e33).
        let mut e_found = false;
        // Whether we've already encountered a digit in this literal.
        let mut digit_found = Self::is_digit(first);

        // Whether the next character in this literal can be an underscore. Numeric literals can
        // include underscores, but underscores are not allowed in the following positions:
        // - directly after a dot ('.').
        // - directly after another underscore ('_').
        // - at the end of the number.
        let mut underscore_allowed = Self::is_digit(first);

        let mut c = self.peek();

        // Numeric literals can have prefixes, in the form of "0x", "0b" or "0o".
        if c.is_none() == false && first == '0' {
            match c.unwrap() {
                'x' | 'b' | 'o' => {
                    digit_found = false; // the '0' we took for a digit isn't actually a digit.
                    self.advance();
                    c = self.peek();
                }
                _ => {}
            }
        }

        let mut ends_in_e = false;
        // Parse the body of the numeric literal. This breaks when the body ends.
        loop {
            // If there's no character, there's no more number.
            if c.is_none() {
                break;
            }

            match c.unwrap() {
                '.' => {
                    // If this isn't the first dot we found in the number, the number ends before it.
                    // This isn't necessarily an error, as Judith allows accessing numbers
                    // (e.g. 7.str()).
                    if dot_found {
                        break;
                    }

                    // We need to check the character after the dot to determine if it's acting as
                    // a decimal point. It's only acting as such if the next character is a number
                    // between 0 and 9. A through F is not allowed here.
                    let c2 = self.peek_next();
                    if c2.is_none() || Self::is_digit(c2.unwrap()) == false {
                        break;
                    }

                    dot_found = true;
                },
                'e' => {
                    if e_found {
                        let cursor = self.cursor();
                        self.error(compiler_messages::Lexer::invalid_number(SourceSpan {
                            start: self.start as i64,
                            end: cursor as i64,
                            line: self.line as i64
                        }, self.extract_lexeme(self.start, cursor)));
                    }
                    e_found = true;
                    ends_in_e = true;
                }
                '_' => {
                    if underscore_allowed == false {
                        let cursor = self.cursor();
                        self.error(compiler_messages::Lexer::invalid_number(SourceSpan {
                            start: self.start as i64,
                            end: cursor as i64,
                            line: self.line as i64
                        }, self.extract_lexeme(self.start, cursor)));
                    }

                    // Can't chain two underscores together, so the next character cannot be an
                    // underscore.
                    underscore_allowed = false;
                }
                _ if Self::is_hex_digit(c.unwrap()) == false => {
                    break;
                }
                _ => {
                    underscore_allowed = true;
                    digit_found = true;
                    ends_in_e = false;
                }
            }

            self.advance();
            c = self.peek();
        }

        if digit_found == false || ends_in_e {
            let cursor = self.cursor();
            self.error(compiler_messages::Lexer::invalid_number(SourceSpan {
                start: self.start as i64,
                end: cursor as i64,
                line: self.line as i64
            }, self.extract_lexeme(self.start, cursor)));
        }

        // Parse the suffix, if any.
        c = self.peek();
        // If the number is followed by a letter (other than "e"), then we've got a suffix. Note that
        // the previous step of this scan doesn't consume trailing dots, so we are guaranteed this
        // character is not part of a member access.
        if c.is_none() == false && c != Some('e') && c != Some('E') && Self::is_letter(c.unwrap()) {
            self.advance();
            c = self.peek();

            // We keep consuming characters until we find something that isn't a letter or number.
            // This will consume prefixes like "f32".
            while c.is_none() == false && (Self::is_letter(c.unwrap()) || Self::is_digit(c.unwrap())) {
                self.advance();
                c = self.peek();
            }
        }

        self.make_token(TokenKind::Number)
    }

    /// Scans a string token until its end. This method assumes that everything up to the FIRST
    /// quote (included) in the string literal has been consumed already. For example, for the token
    /// |"test"|, only |"| has been already consumed. For a string with flags, like |f"test"|, |f"|
    /// has already been consumed. For a string with multiple delimiting quotes, like |ff\`\`\`test```|,
    /// |ff\`| (all flags and the first quote) has already been consumed.
    /// * `quoting_char` The character used to start the string (either '"' or '`').
    /// * `start_column` The column the where the first delimiter of the string is.
    fn scan_string (&mut self, quoting_char: char, start_column: usize) -> Token {
        let mut opening_quotes = 1; // the one that triggered this scan.
        while let Some(c) = self.peek() {
            if c == quoting_char {
                opening_quotes += 1;
                self.advance();
            }
            else {
                break;
            }
        }

        // Exactly two quotes is the empty string ("" or ``).
        if opening_quotes == 2 {
            return self.make_string_token(quoting_char, 1, start_column);
        }

        // The string will only end when it encounters as many quoting chars in a row as those used
        // to start the string. Plainly speaking, if a string starts with |""""|, it ends when we
        // encounter another |""""|.
        let mut closing_quotes = 0;

        while closing_quotes < opening_quotes {
            if self.is_at_end() {
                let cursor = self.cursor() as i64;
                self.error(compiler_messages::Lexer::unterminated_string(SourceSpan {
                    start: self.start as i64,
                    end: cursor,
                    line: self.line as i64
                }));
                return self.make_token(TokenKind::Invalid);
            }

            if self.advance() == Some(quoting_char) {
                closing_quotes += 1;
            }
            else {
                closing_quotes = 0;
            }
        }

        self.make_string_token(quoting_char, opening_quotes, start_column)
    }

    fn scan_literal_like (&mut self) -> Token {
        let start_column = self.column - 1;

        // We may be scanning an identifier, a keyword or the flags at the start of a string literal
        // (e.g. ef"My string").
        loop {
            match self.peek() {
                Some(c) if c == '"' || c == '`' => {
                    return self.scan_string(c, start_column);
                }
                Some(c) if Self::is_identifier_char(c) => {
                    self.advance();
                }
                _ => break,
            }
        }

        let cursor = self.cursor();
        let lexeme = self.extract_lexeme(self.start, cursor);
        match KEYWORD_MAP.get(&lexeme) {
            Some(kw) => self.make_token(kw),
            None => self.make_token(TokenKind::Identifier),
        }
    }

    /// Scans whitespace trivia. This doesn't care whether any whitespace has already been consumed
    /// or not.
    fn scan_whitespace_trivia (&mut self) -> Trivia {
        while let Some(c) = self.peek() {
            if Self::is_whitespace(c) == false {
                break;
            }
            self.advance();
        }

        self.make_trivia(TriviaKind::Whitespace)
    }

    /// Scans a multiline comment, assuming the cursor is already past the initial "--!".
    fn scan_multiline_comment(&mut self) -> Trivia {
        let mut last_char = None;

        while let Some(c) = self.advance() {
            if last_char == Some('-') && c == '-' {
                break;
            }

            last_char = Some(c);
        }

        self.make_trivia(TriviaKind::MultiLineComment)
    }

    /// Scans a single line comment, assuming the cursor is already past the initial "--".
    fn scan_single_line_comment(&mut self) -> Trivia {
        while let Some(c) = self.peek() {
            // Keep going until there's a line break. There the comment ends.
            if Self::is_newline(c) {
                break;
            }

            // We advance here so we don't consume newline characters (they are their own trivia).
            self.advance();
        }

        self.make_trivia(TriviaKind::SingleLineComment)
    }

    fn make_token(&mut self, kind: TokenKind) -> Token {
        let cursor = self.cursor();
        Token::Regular(RegularToken {
            kind,
            lexeme: self.extract_lexeme(self.start, cursor),
            start: self.start as i64,
            end: self.cursor() as i64,
            line: self.line as i64,
            leading_trivia: vec![], // TODO: This is dirty.
            trailing_trivia: vec![],
        })
    }

    fn make_string_token(&mut self, quoting_char: char, quote_count: i32, column: usize) -> Token {
        let cursor = self.cursor();

        Token::String(StringToken {
            base: RegularToken {
                kind: TokenKind::String,
                lexeme: self.extract_lexeme(self.start, cursor),
                start: self.start as i64,
                end: cursor as i64,
                line: self.line as i64,
                leading_trivia: vec![], // TODO: This is dirty.
                trailing_trivia: vec![],
            },
            string_kind: match quote_count {
                1 => StringLiteralKind::Regular,
                _ => StringLiteralKind::Raw,
            },
            delimiter: quoting_char,
            delimiter_count: quote_count,
            column: column as i32,
        })
    }

    fn make_trivia(&mut self, kind: TriviaKind) -> Trivia {
        let cursor = self.cursor();
        let lexeme = self.extract_lexeme(self.start, cursor);

        Trivia {
            kind,
            lexeme,
            span: SourceSpan::new(self.start as i64, self.cursor() as i64, self.line as i64),
        }
    }

    /// Marks the lexer as containing errors and adds the message to the container.
    fn error(&mut self, msg: CompilerMessage) {
        self.hasErrors = true;
        self.messages.add(msg);
    }

    // region Helper functions
    /// Moves the character forwards
    fn move_chars_forwards (&mut self) -> Option<char> {
        if let Some((_, char)) = self.chars.next() {
            self.column += 1;

            return Some(char);
        }
        else {
            return None;
        }
    }

    fn cursor(&mut self) -> usize {
        if let Some((index, _)) = self.chars.peek() {
            return *index;
        }
        self.src.len()
    }

    /// Returns true if the cursor has reached the end of the source.
    #[inline(always)]
    fn is_at_end(&mut self) -> bool {
        self.chars.peek().is_none()
    }

    /// Returns the character at the cursor's position and moves it forward. If a newline is found,
    /// recognizes the newline style (\r, \n or \r\n) and advances to the start of the next line,
    /// but only returns the first character in the sequence.
    fn advance(&mut self) -> Option<char> {
        let char = self.move_chars_forwards();

        // Update line and column if we advanced past a newline.
        match char {
            // Newline: either Windows's \r\n or old Mac's \r.
            Some('\r') => {
                self.line += 1;
                self.column = FIRST_COLUMN;

                // We have "\r\n", so we advance past the \n, too.
                if self.peek() == Some('\n') {
                    self.move_chars_forwards();
                }
            }
            // Newline: Linux's \n.
            Some('\n') => {
                self.line += 1;
                self.column = FIRST_COLUMN;
            }
            // No newline.
            _ => {}
        }

        char
    }

    /// Checks the character the cursor is at. If it matches the character given, moves the cursor
    /// forward. returns true when the character given was successfully matched.
    fn try_match(&mut self, expected: char) -> bool {
        if self.is_at_end() {
            return false;
        }

        let char = self.peek();

        if let Some(c) = char {
            if c == expected {
                self.move_chars_forwards();
                return true;
            }
        }

        false
    }

    /// Returns the character at the cursor's position, without moving the cursor.
    fn peek (&mut self) -> Option<char> {
        if self.is_at_end() {
            return None;
        }

        if let Some((_, char)) = self.chars.peek() {
            Some(*char)
        }
        else {
            None
        }
    }

    /// Returns the next character after the cursor's position, without moving the cursor.
    fn peek_next (&mut self) -> Option<char> {
        let mut slice = self.src[self.cursor()..].chars();
        slice.next();

        slice.next()
    }

    /// Creates a new string with contents of the source string between the start (inclusive) and
    /// end (exclusive) given.
    fn extract_lexeme (&self, start: usize, end: usize) -> String {
        return self.src[start..end].to_string();
    }
    // endregion

    // region Test characters
    /// Returns true if the character given is a space or a tab.
    #[inline(always)]
    fn is_whitespace (c: char) -> bool {
        c == ' ' || c == '\t'
    }

    /// Returns true if the character given is a newline character.
    fn is_newline (c: char) -> bool {
        c == '\n' || c == '\r'
    }

    /// Returns true if the character given is a digit.
    #[inline(always)]
    fn is_digit (c: char) -> bool {
        c >= '0' && c <= '9'
    }

    /// Returns true if the character given is a hexadecimal digit.
    #[inline(always)]
    fn is_hex_digit (c: char) -> bool {
        Self::is_digit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')
    }

    /// Returns true if the character given is an ASCII letter.
    #[inline(always)]
    fn is_letter (c: char) -> bool {
        (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')
    }

    /// Returns true if the character given can be the first character in a numeric literal. This
    /// excludes the minus sign.
    #[inline(always)]
    fn is_number_leading_char (c: char) -> bool {
        Self::is_digit(c) || c == '.'
    }

    /// Returns true if the character given can be the first character in an identifier. This
    /// includes the identifier escape character ('\').
    #[inline(always)]
    fn is_identifier_leading_char (c: char) -> bool {
        Self::is_letter(c) || c == '_' || c == '\\'
    }

    /// Returns true if the character given can be part of an identifier in any part other than the
    /// leading character in it. This is ASCII letters, numbers and underscore.
    #[inline(always)]
    fn is_identifier_char (c: char) -> bool {
        Self::is_letter(c) || c == '_' || Self::is_digit(c)
    }
    // endregion
}

pub fn tokenize(src: &str) -> LexerResult {
    let mut lexer = Lexer::new(src);
    let mut tokens: Vec<Token> = vec![];

    while tokens.len() == 0 || tokens.last().unwrap().kind() != TokenKind::EOF {
        tokens.push(lexer.next_token());
    }

    LexerResult {
        tokens,
        messages: lexer.messages,
    }
}

#[cfg(test)]
mod tests {
    use crate::judith::compiler_messages::MessageCode;
    use super::*;

    #[test]
    fn test_helper_fns() {
        println!("Testing helper functions.");

        let mut lexer = Lexer::new("do end while if");
        let peek = lexer.peek();
        let cursor = lexer.cursor();

        assert_eq!(peek, Some('d'), "When we start, we should be able to peek() the first character.");
        assert_eq!(cursor, 0);

        let advance = lexer.advance();
        assert_eq!(advance, peek, "Value returned by advance() should be what we peek()ed before.");

        let peek = lexer.peek();
        let cursor = lexer.cursor();

        assert_eq!(peek, Some('o'));
        assert_eq!(cursor, 1);

        let try_match = lexer.try_match('!'); // This shouldn't match.
        let peek = lexer.peek();
        let cursor = lexer.cursor();

        assert_eq!(try_match, false);
        assert_eq!(peek, Some('o')); // same as before.
        assert_eq!(cursor, 1);

        let try_match = lexer.try_match('o'); // This should match.
        let peek = lexer.peek();
        let cursor = lexer.cursor();

        assert_eq!(try_match, true);
        assert_eq!(peek, Some(' ')); // same as before.
        assert_eq!(cursor, 2);
    }

    #[test]
    /// Test
    fn test_keywords() {
        let cases = vec![
            ("and", TokenKind::KwAnd),
            ("break", TokenKind::KwBreak),
            ("do", TokenKind::KwDo),
            ("__p_print", TokenKind::PkwPrint),
        ];

        for (input, expected) in cases {
            println!("Testing '{}'.", input);

            let res = tokenize(input);
            assert_eq!(res.tokens.len(), 2); // Should contain [keyword, EOF].
            assert_eq!(res.tokens[0].kind(), expected);
        }
    }

    #[test]
    fn test_simple_tokens() {
        println!("Testing simple tokens.");

        println!("Testing '+'");
        let res = tokenize("+");
        assert_eq!(res.tokens.len(), 2);
        assert_eq!(res.tokens[0].kind(), TokenKind::Plus);

        println!("Testing '- -'");
        let res = tokenize("- -");
        println!("{:?}", res.tokens[0]);
        println!("{:?}", res.tokens[1]);
        assert_eq!(res.tokens.len(), 3);
        assert_eq!(res.tokens[0].kind(), TokenKind::Minus);
        assert_eq!(res.tokens[1].kind(), TokenKind::Minus);

        println!("Testing '--'");
        let res = tokenize("--");
        assert_eq!(res.tokens.len(), 1);
        assert_eq!(res.tokens[0].kind(), TokenKind::EOF);
        assert_eq!(res.tokens[0].base().leading_trivia.len(), 1);
        assert_eq!(res.tokens[0].base().leading_trivia[0].kind, TriviaKind::SingleLineComment);

        println!("Testing '= >'");
        let res = tokenize("= >");
        assert_eq!(res.tokens.len(), 3);
        assert_eq!(res.tokens[0].kind(), TokenKind::Equal);
        assert_eq!(res.tokens[1].kind(), TokenKind::Greater);

        println!("Testing '=>'");
        let res = tokenize("=>");
        println!("{:?}", res.tokens[0].kind());
        assert_eq!(res.tokens.len(), 2);
        assert_eq!(res.tokens[0].kind(), TokenKind::EqualArrow);
    }

    #[test]
    fn test_correct_numeric_literals() {
        let cases = vec![
            ("42", "42"),
            ("5", "5"),
            ("05", "05"),
            ("005", "005"),
            ("500", "500"),
            ("-42", "-42"),
            ("-042", "-042"),
            ("0_5", "0_5"),
            ("0_50_", "0_50_"),
            ("0.5", "0.5"),
            (".5", ".5"),
            ("369_", "369_"),
            ("100_i32", "100_i32"),
            ("0x0123456789abcDEf", "0x0123456789abcDEf"),
            ("0x0ffu8", "0x0ffu8"),
            ("0b01101", "0b01101"),
            ("0o03245", "0o03245"),
            ("0_.2", "0_.2"),
            ("4.1e11", "4.1e11"),
            ("0", "0"),
            ("0.0", "0.0"),
            ("0e0", "0e0"),
            ("1u8", "1u8"),
            ("1.0e0", "1.0e0"),
            ("0x1", "0x1"),
            ("1_000_000.999_999_999e9_999f64", "1_000_000.999_999_999e9_999f64"),
            ("0xA_B_C_D_1_2_3.0p10", "0xA_B_C_D_1_2_3.0p10"),
            ("0b1_0_1_1_1_0_1_0_0_1_1_0_0_1_u64", "0b1_0_1_1_1_0_1_0_0_1_1_0_0_1_u64"),
            ("0o7_7_7_7_7_7_7_7_u128", "0o7_7_7_7_7_7_7_7_u128"),
        ];

        for (input, expected) in cases {
            println!("Testing '{}'.", input);

            let res = tokenize(input);
            assert_eq!(res.tokens.len(), 2); // Should contain [keyword, EOF].
            assert_eq!(res.tokens[0].kind(), TokenKind::Number);
            assert_eq!(res.tokens[0].base().lexeme, expected);
            if res.messages.count() != 0 {
                res.messages.dump_all();
            }
            assert_eq!(res.messages.count(), 0);
        }
    }
    #[test]
    fn test_numeric_literal_special_cases() {
        println!("Testing '_123'");

        let res = tokenize("_123");
        assert_eq!(res.tokens.len(), 2);
        assert_eq!(res.tokens[0].kind(), TokenKind::Identifier);
        assert_eq!(res.tokens[0].base().lexeme, "_123");

        println!("Testing '1__2'");
        let res = tokenize("1__2");
        assert_eq!(res.tokens.len(), 2);
        assert_eq!(res.messages.errors.len(), 1);
        assert_eq!(res.messages.errors[0].code, MessageCode::InvalidNumber { lexeme: "1_".to_string()});

        println!("Testing '0x'");
        let res = tokenize("0x");
        assert_eq!(res.tokens.len(), 2);
        assert_eq!(res.messages.errors.len(), 1);
        assert_eq!(res.messages.errors[0].code, MessageCode::InvalidNumber { lexeme: "0x".to_string()});

        println!("Testing '0ou8'");
        let res = tokenize("0ou8");
        assert_eq!(res.tokens.len(), 2);
        assert_eq!(res.messages.errors.len(), 1);
        assert_eq!(res.messages.errors[0].code, MessageCode::InvalidNumber { lexeme: "0o".to_string()});

        println!("Testing '5eu8'");
        let res = tokenize("5eu8");
        assert_eq!(res.tokens.len(), 2);
        assert_eq!(res.messages.errors.len(), 1);
        assert_eq!(res.messages.errors[0].code, MessageCode::InvalidNumber { lexeme: "5e".to_string()});

        println!("Testing '5ee");
        let res = tokenize("5ee");
        assert_eq!(res.tokens.len(), 2);
        assert_eq!(res.messages.errors.len(), 2);
        assert_eq!(res.messages.errors[0].code, MessageCode::InvalidNumber { lexeme: "5e".to_string()}); // this one for having two 'e'.
        assert_eq!(res.messages.errors[1].code, MessageCode::InvalidNumber { lexeme: "5ee".to_string()}); // this one for not having anything after the 'e'.

        println!("Testing '5ee5");
        let res = tokenize("5ee5");
        assert_eq!(res.tokens.len(), 2);
        assert_eq!(res.messages.errors.len(), 1);
        assert_eq!(res.messages.errors[0].code, MessageCode::InvalidNumber { lexeme: "5e".to_string()});

        println!("Testing '5e3e1");
        let res = tokenize("5e3e1");
        assert_eq!(res.tokens.len(), 2);
        assert_eq!(res.messages.errors.len(), 1);
        assert_eq!(res.messages.errors[0].code, MessageCode::InvalidNumber { lexeme: "5e3".to_string()});

        println!("Testing '5.3.1");
        let res = tokenize("5.3.1");
        assert_eq!(res.tokens.len(), 3);
        assert_eq!(res.tokens[0].kind(), TokenKind::Number);
        assert_eq!(res.tokens[0].base().lexeme, "5.3");
        assert_eq!(res.tokens[1].kind(), TokenKind::Number);
        assert_eq!(res.tokens[1].base().lexeme, ".1");
        assert_eq!(res.messages.errors.len(), 0);

        println!("Testing '1.");

        let res = tokenize("1.");
        assert_eq!(res.tokens.len(), 3);
        assert_eq!(res.tokens[0].kind(), TokenKind::Number);
        assert_eq!(res.tokens[0].base().lexeme, "1");
        assert_eq!(res.tokens[1].kind(), TokenKind::Dot);
        assert_eq!(res.tokens[1].base().lexeme, ".");
    }

    #[test]
    fn test_leading_and_trailing_trivia () {
        // Note: we also test that the EOF has no leading or trailing trivia.
        let cases = vec![
            ("do", 0, 0),
            ("  if  \n", 1, 2),
            ("\n --! comment -- while\n", 4, 1)
            //("\n\n\nelse\n do", )
        ];

        for (input, leading_count, trailing_count) in cases {
            let res = tokenize(input); // should have 1 leading (space) and 2 trailing (space, newline).
            assert_eq!(res.tokens.len(), 2); // Should contain [if, EOF].

            println!("Testing token's trivia.");
            assert_eq!(res.tokens[0].base().leading_trivia.len(), leading_count, "Incorrect leading trivia count.");
            assert_eq!(res.tokens[0].base().trailing_trivia.len(), trailing_count, "Incorrect trailing trivia count.");

            println!("Testing EOF's trivia.");
            assert_eq!(res.tokens[1].base().leading_trivia.len(), 0, "Incorrect EOF leading trivia count.");
            assert_eq!(res.tokens[1].base().trailing_trivia.len(), 0, "Incorrect EOF trailing trivia count (????).");
        }
    }

    #[test]
    fn test_comment_trivia () {
        println!("Testing first comment.");
        let res = tokenize("-- comment until next line\n`backticks`");

        assert_eq!(res.tokens[0].base().leading_trivia.len(), 2);
        assert_eq!(res.tokens[0].base().leading_trivia[0].kind, TriviaKind::SingleLineComment);
        assert_eq!(res.tokens[0].base().leading_trivia[0].lexeme, "-- comment until next line");
        assert_eq!(res.tokens[0].base().leading_trivia[1].kind, TriviaKind::LineBreak);
        println!("Testing second comment.");
        let res = tokenize("--! com -- --! com2 -- -- com\n--com do\nelse");

        assert_eq!(res.tokens[0].base().leading_trivia.len(), 8);
        assert_eq!(res.tokens[0].kind(), TokenKind::KwElse);

        assert_eq!(res.tokens[0].base().leading_trivia[0].kind, TriviaKind::MultiLineComment);
        assert_eq!(res.tokens[0].base().leading_trivia[0].lexeme, "--! com --");

        assert_eq!(res.tokens[0].base().leading_trivia[1].kind, TriviaKind::Whitespace);
        assert_eq!(res.tokens[0].base().leading_trivia[1].lexeme, " ");

        assert_eq!(res.tokens[0].base().leading_trivia[2].kind, TriviaKind::MultiLineComment);
        assert_eq!(res.tokens[0].base().leading_trivia[2].lexeme, "--! com2 --");

        assert_eq!(res.tokens[0].base().leading_trivia[3].kind, TriviaKind::Whitespace);
        assert_eq!(res.tokens[0].base().leading_trivia[3].lexeme, " ");

        assert_eq!(res.tokens[0].base().leading_trivia[4].kind, TriviaKind::SingleLineComment);
        assert_eq!(res.tokens[0].base().leading_trivia[4].lexeme, "-- com");

        assert_eq!(res.tokens[0].base().leading_trivia[5].kind, TriviaKind::LineBreak);
        assert_eq!(res.tokens[0].base().leading_trivia[5].lexeme, "\n");

        assert_eq!(res.tokens[0].base().leading_trivia[6].kind, TriviaKind::SingleLineComment);
        assert_eq!(res.tokens[0].base().leading_trivia[6].lexeme, "--com do");

        assert_eq!(res.tokens[0].base().leading_trivia[7].kind, TriviaKind::LineBreak);
        assert_eq!(res.tokens[0].base().leading_trivia[7].lexeme, "\n");
    }
}