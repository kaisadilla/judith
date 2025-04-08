use std::iter::{Enumerate, Peekable};
use std::slice::Iter;
use std::str::Chars;
use crate::judith::compiler_messages;
use crate::judith::compiler_messages::{CompilerMessage, MessageContainer};
use crate::judith::lexical::token::{Token, TokenKind};
use crate::judith::syntax::nodes::*;

pub struct Parser<'a> {
    tokens: &'a Vec<Token>,
    iter: Peekable<Enumerate<Iter<'a, Token>>>,
    previous: Option<&'a Token>,
    has_errors: bool,
    messages: MessageContainer,
}

pub struct ParserResult {
    pub nodes: Vec<SyntaxNode>,
    pub messages: MessageContainer,
}

impl<'a> Parser<'a> {
    pub fn new(tokens: &'a Vec<Token>) -> Self {
        Parser {
            tokens,
            iter: tokens.iter().enumerate().peekable(),
            previous: None,
            has_errors: false,
            messages: MessageContainer::new(),
        }
    }

    // region Helper methods
    fn match_token(&mut self, kinds: &[TokenKind]) -> bool {
        for kind in kinds {
            if self.check(kind.clone()) {
                self.advance();
                return true;
            }
        }

        false
    }

    /// Returns true if the current token matches the type given.
    /// * `kind`: The token kind to check for.
    fn check(&mut self, kind: TokenKind) -> bool {
        if self.is_at_end() {
            return false;
        }

        self.peek().unwrap().kind() == kind // Unwrap shouldn't fail, as is_at_end would've returned true.
    }

    fn check_many(&mut self, kinds: &[TokenKind]) -> bool {
        for kind in kinds {
            if self.check(kind.clone()) {
                return true;
            }
        }

        false
    }

    /// Returns `true` if the current token is the EOF token (or if there's no token).
    fn is_at_end(&mut self) -> bool {
        match self.peek() {
            Some(token) => token.kind() == TokenKind::EOF,
            None => true,
        }
    }

    fn cursor(&mut self) -> usize {
        if let Some((index, _)) = self.iter.peek() {
            return *index;
        }
        self.tokens.len()
    }

    /// Returns the current token.
    fn peek(&mut self) -> Option<Token> {
        if let Some((_, tok)) = self.iter.peek() {
            Some(tok.clone().clone())
        }
        else {
            None
        }
    }

    /// Returns the previous token.
    fn peek_previous (&mut self) -> Option<Token> {
        self.previous.map(|t| t.clone())
    }

    /// Returns the current token and moves into the next one.
    fn advance(&mut self) -> Option<Token> {
        if let Some((_, tok)) = self.iter.next() {
            Some(tok.clone())
        }
        else {
            None
        }
    }

    /// Advances (returning the current token) only if the current token is of the kind given.
    fn try_consume(&mut self, kind: TokenKind) -> Option<Token> {
        if self.check(kind) {
            self.advance().map(|t| t.clone())
        }
        else {
            None
        }
    }

    /// Advances (returning the current token) only if the currente token matches one of the kinds
    /// given.
    fn try_consume_many(&mut self, kinds: &[TokenKind]) -> Option<Token> {
        if self.check_many(kinds) {
            self.advance().map(|t| t.clone())
        }
        else {
            None
        }
    }
    // endregion

    // region Consumer methods
    pub fn try_consume_top_level_node (&mut self) -> Option<SyntaxNode> {
        if let Some(expr) = self.try_consume_expr() {
            Some(SyntaxNode::Expr(expr))
        }
        else {
            None
        }
    }

    pub fn try_consume_expr(&mut self) -> Option<Expr> {
        if let Some(expr) = self.try_consume_primary() {
            Some(expr)
        }
        else {
            None
        }
    }

    pub fn try_consume_primary(&mut self) -> Option<Expr> {
        if let Some(group_expr) = self.try_consume_group_expr() {
            Some(group_expr)
        }
        else if let Some(literal_expr) = self.try_consume_literal_expr() {
            Some(literal_expr)
        }
        else {
            None
        }
    }

    pub fn try_consume_group_expr(&mut self) -> Option<Expr> {
        let left_paren = self.try_consume(TokenKind::LeftParen)?;

        let tok = self.peek().unwrap().clone(); // TODO: Do not use unwrap.
        let expr = match self.try_consume_expr() {
            Some(expr) => expr,
            None => {
                self.error(compiler_messages::Parser::expression_expected(tok));
                return None;
            }
        };

        let tok = self.peek().unwrap().clone(); // TODO: Do not use unwrap.
        let right_paren = match self.try_consume(TokenKind::RightParen) {
            Some(expr) => expr,
            None => {
                self.error(compiler_messages::Parser::right_paren_expected(tok));
                return None;
            }
        };

        Some(Expr::Group(
            Box::from(SyntaxFactory::group_expr(left_paren.clone(), expr, right_paren.clone()))
        ))
    }

    pub fn try_consume_literal_expr(&mut self) -> Option<Expr> {
        if let Some(literal) = self.try_consume_literal() {
            Some(Expr::Literal(Box::from(SyntaxFactory::literal_expr(literal))))
        }
        else {
            None
        }
    }

    pub fn try_consume_literal(&mut self) -> Option<Literal> {
        if let Some(tok) = self.try_consume(TokenKind::Number) {
            Some(SyntaxFactory::literal(tok.clone()))
        }
        else {
            None
        }
    }
    // endregion

    /// Marks the lexer as containing errors and adds the message to the container.
    fn error(&mut self, msg: CompilerMessage) {
        self.has_errors = true;
        self.messages.add(msg);
    }
}

pub fn parse(tokens: Vec<Token>) -> ParserResult {
    let mut parser = Parser::new(&tokens);

    let mut nodes: Vec<SyntaxNode> = vec![];

    while let Some(node) = parser.try_consume_top_level_node() {
        nodes.push(node);
    }

    ParserResult {
        nodes,
        messages: parser.messages,
    }
}


/*pub fn tokenize(src: &str) -> Vec<Token>  {
    let mut lexer = Lexer::new(src);
    let mut tokens: Vec<Token> = vec![];

    while tokens.len() == 0 || tokens.last().unwrap().kind() != TokenKind::EOF {
        tokens.push(lexer.next_token());
    }

    tokens
}*/