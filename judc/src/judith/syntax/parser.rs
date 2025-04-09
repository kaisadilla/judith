use std::iter::{Enumerate, Peekable};
use std::slice::Iter;
use crate::judith::compiler_messages;
use crate::judith::compiler_messages::{CompilerMessage, MessageContainer};
use crate::judith::lexical::token::{Token, TokenKind};
use crate::judith::syntax::nodes::*;
use crate::judith::syntax::syntax_factory::SyntaxFactory;

pub struct Parser<'a> {
    tokens: &'a Vec<Token>,
    iter: Peekable<Enumerate<Iter<'a, Token>>>,
    previous: Option<&'a Token>,
    has_errors: bool,
    messages: MessageContainer,
}

/// The result of parsing a list of tokens.
pub struct ParserResult {
    pub nodes: Vec<SyntaxNode>,
    pub messages: MessageContainer,
}

/// The result of trying to parse a specific kind of node.
pub enum ParseAttempt<T> {
    /// The expected node wasn't found.
    None,
    /// The expected node was found and parsed correctly.
    Ok(T),
    /// The expected node was found, but its syntax was incorrect.
    Err(CompilerMessage),
}

impl<'a> Parser<'a> {
    pub fn new(tokens: &'a Vec<Token>) -> Self {
        if tokens.len() == 0 || tokens.last().unwrap().kind() != TokenKind::EOF {
            panic!("The list of tokens must be ended by an EOF token.");
        }

        Parser {
            tokens,
            iter: tokens.iter().enumerate().peekable(),
            previous: None,
            has_errors: false,
            messages: MessageContainer::new(),
        }
    }

    // region Helper methods
    fn last_token (&self) -> Token {
        self.tokens.last().unwrap().clone() // There has to be at least one token, if the Parser was built properly.
    }

    fn match_token (&mut self, kinds: &[TokenKind]) -> bool {
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
    fn check (&mut self, kind: TokenKind) -> bool {
        if self.is_at_end() {
            return false;
        }

        self.peek().unwrap().kind() == kind // Unwrap shouldn't fail, as is_at_end would've returned true.
    }

    fn check_many (&mut self, kinds: &[TokenKind]) -> bool {
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
            Some((*tok).clone())
        }
        else {
            None
        }
    }

    /// Returns the previous token.
    fn peek_previous (&mut self) -> Option<Token> {
        self.previous.map(|t| t.clone())
    }

    /// Returns the current token, or the EOF token if there's no tokens.
    fn now (&mut self) -> Token {
        self.peek().unwrap_or(self.last_token())
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

    // region Parse methods
    pub fn parse_top_level_node (&mut self) -> ParseAttempt<SyntaxNode> {
        match self.parse_expr() {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(SyntaxNode::Expr(expr)),
            ParseAttempt::Err(err) => return self.register_err_node(err),
            _ => {},
        };

        ParseAttempt::None
    }

    pub fn parse_expr (&mut self) -> ParseAttempt<Expr> {
        match self.parse_access_expr() {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(expr),
            ParseAttempt::Err(err) => return self.register_err_expr(err),
            _ => {},
        };

        ParseAttempt::None
    }

    pub fn parse_access_expr (&mut self) -> ParseAttempt<Expr> {
        // Because member access can be implicit (e.g. '.name'), we cannot discard a member access
        // just because we didn't encounter what is being accessed.
        let mut receiver = match self.parse_primary() {
            ParseAttempt::Ok(expr) => Some(expr),
            ParseAttempt::None => None,
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
        };

        // Now, if we don't find a member access token ('.'):
        let tok = self.peek();
        if tok.is_none() || tok.unwrap().kind() != TokenKind::Dot {
            // If we didn't find a provider, then we aren't parsing an access expression (or anything
            // below it). If we did, then the provider is the expression we parsed.
            return match receiver {
                Some(expr) => ParseAttempt::Ok(expr),
                None => ParseAttempt::None,
            };
        }

        while let ParseAttempt::Ok(op) = self.parse_operator(&[TokenKind::Dot]) {
            let tok = self.now();
            let name = match self.parse_simple_identifier() {
                ParseAttempt::Ok(name) => name,
                ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
                ParseAttempt::None => {
                    return ParseAttempt::Err(compiler_messages::Parser::identifier_expected(tok));
                }
            };

            receiver = Some(Expr::Access(
                Box::from(SyntaxFactory::access_expr(receiver, op, name))
            ));
        }

        match receiver {
            Some(expr) => ParseAttempt::Ok(expr),
            None => ParseAttempt::None,
        }
    }

    pub fn parse_primary (&mut self) -> ParseAttempt<Expr> {
        match self.parse_group_expr() {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(expr),
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            _ => {},
        };

        match self.parse_identifier_expr() {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(expr),
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            _ => {},
        };

        match self.parse_literal_expr() {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(expr),
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            _ => {},
        };

        ParseAttempt::None
    }

    pub fn parse_group_expr (&mut self) -> ParseAttempt<Expr> {
        let left_paren = match self.try_consume(TokenKind::LeftParen) {
            Some(tok) => tok,
            None => return ParseAttempt::None,
        };

        let tok = self.now();
        let expr = match self.parse_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            ParseAttempt::None => {
                return ParseAttempt::Err(compiler_messages::Parser::identifier_expected(tok));
            }
        };


        let tok = self.now();
        let right_paren = match self.try_consume(TokenKind::RightParen) {
            Some(tok) => tok,
            None => {
                return ParseAttempt::Err(compiler_messages::Parser::right_paren_expected(tok));
            }
        };

        ParseAttempt::Ok(Expr::Group(
            Box::from(SyntaxFactory::group_expr(left_paren.clone(), expr, right_paren.clone()))
        ))
    }

    pub fn parse_identifier_expr (&mut self) -> ParseAttempt<Expr> {
        let id = match self.parse_qualified_identifier() {
            ParseAttempt::Ok(id) => id,
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            ParseAttempt::None => return ParseAttempt::None,
        };

        ParseAttempt::Ok(Expr::Identifier(
            Box::from(SyntaxFactory::identifier_expr(id))
        ))
    }

    pub fn parse_literal_expr (&mut self) -> ParseAttempt<Expr> {
        match self.parse_literal() {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(Expr::Literal(
                Box::from(SyntaxFactory::literal_expr(expr))
            )),
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            _ => {}
        }

        ParseAttempt::None
    }

    pub fn parse_simple_identifier (&mut self) -> ParseAttempt<SimpleIdentifier> {
        let id_tok = match self.try_consume(TokenKind::Identifier) {
            Some(id) => id,
            None => return ParseAttempt::None,
        };

        ParseAttempt::Ok(SyntaxFactory::simple_identifier(id_tok))
    }

    pub fn parse_qualified_identifier (&mut self) -> ParseAttempt<Identifier> {
        let simple_id = match self.parse_simple_identifier() {
            ParseAttempt::Ok(id) => id,
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            ParseAttempt::None => return ParseAttempt::None,
        };

        let mut identifier: Identifier = Identifier::Simple(simple_id);

        while let ParseAttempt::Ok(op) = self.parse_operator(&[TokenKind::DoubleColon]) {
            let tok = self.now();
            let name = match self.parse_simple_identifier() {
                ParseAttempt::Ok(id) => id,
                ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
                ParseAttempt::None => {
                    return ParseAttempt::Err(compiler_messages::Parser::identifier_expected(tok));
                }
            };

            identifier = Identifier::Qualified(
                Box::from(SyntaxFactory::qualified_identifier(identifier, op, name))
            );
        }

        ParseAttempt::Ok(identifier)
    }

    pub fn parse_literal (&mut self) -> ParseAttempt<Literal> {
        if let Some(tok) = self.try_consume(TokenKind::KwTrue) {
            ParseAttempt::Ok(SyntaxFactory::literal(tok.clone()))
        }
        else if let Some(tok) = self.try_consume(TokenKind::KwFalse) {
            ParseAttempt::Ok(SyntaxFactory::literal(tok.clone()))
        }
        else if let Some(tok) = self.try_consume(TokenKind::Number) {
            ParseAttempt::Ok(SyntaxFactory::literal(tok.clone()))
        }
        else if let Some(tok) = self.try_consume(TokenKind::String) {
            ParseAttempt::Ok(SyntaxFactory::literal(tok.clone()))
        }
        else {
            ParseAttempt::None
        }
    }

    pub fn parse_operator (&mut self, kinds: &[TokenKind]) -> ParseAttempt<Operator> {
        let op_tok = self.try_consume_many(kinds);
        if op_tok.is_none() {
            return ParseAttempt::None;
        }

        ParseAttempt::Ok(SyntaxFactory::operator(op_tok.unwrap()))
    }
    // endregion

    /// Marks the lexer as containing errors and adds the message to the container.
    fn error (&mut self, msg: CompilerMessage) {
        self.has_errors = true;
        self.messages.add(msg);
    }

    fn register_err_expr (&mut self, err: CompilerMessage) -> ParseAttempt<Expr> {
        self.messages.add(err);

        ParseAttempt::Ok(Expr::Error(SyntaxFactory::error_node()))
    }

    fn register_err_node (&mut self, err: CompilerMessage) -> ParseAttempt<SyntaxNode> {
        self.messages.add(err);

        ParseAttempt::Ok(SyntaxNode::Error(SyntaxFactory::error_node()))
    }
}

pub fn parse(tokens: Vec<Token>) -> ParserResult {
    let mut parser = Parser::new(&tokens);

    let mut nodes: Vec<SyntaxNode> = vec![];

    while let node = parser.parse_top_level_node() {
        if let ParseAttempt::Ok(node) = node {
            nodes.push(node);
        }
        else if let ParseAttempt::None = node {
            break;
        }
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