use std::iter::Peekable;
use std::slice::Iter;
use crate::judith::lexical::token::{Token, TokenKind};
use crate::judith::syntax::nodes::*;

pub struct Parser<'a> {
    tokens: &'a Vec<Token>,
    iter: Peekable<Iter<'a, Token>>,
    previous: Option<&'a Token>,
}

impl<'a> Parser<'a> {
    pub fn new(tokens: &'a Vec<Token>) -> Self {
        Parser {
            tokens,
            iter: tokens.iter().peekable(),
            previous: None,
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

    /// Returns the current token.
    fn peek(&mut self) -> Option<&Token> {
        self.iter.peek().cloned()
    }

    /// Returns the previous token.
    fn peek_previous (&mut self) -> Option<&Token> {
        self.previous
    }

    /// Returns the current token and moves into the next one.
    fn advance(&mut self) -> Option<&Token> {
        self.iter.next()
    }

    /// Advances (returning the current token) only if the current token is of the kind given.
    fn try_consume(&mut self, kind: TokenKind) -> Option<&Token> {
        if self.check(kind) {
            self.advance()
        }
        else {
            None
        }
    }

    /// Advances (returning the current token) only if the currente token matches one of the kinds
    /// given.
    fn try_consume_many(&mut self, kinds: &[TokenKind]) -> Option<&Token> {
        if self.check_many(kinds) {
            self.advance()
        }
        else {
            None
        }
    }
    // endregion

    // region Consumer methods
    //pub fn try_consume_top_level_node (&mut self) -> Option<SyntaxNode> {
    //
    //}
    // endregion

    pub fn parse (&mut self) {
        let mut nodes: Vec<SyntaxNode>;

        //while let Some(node) = self.try_consume_top_level_node() {
        //    nodes.push(node);
        //}
    }
}

//pub fn parse(tokens: Vec<Token>) -> Expr {
//    let mut parser = Parser::new(&tokens);
//
//
//    A
//}


/*pub fn tokenize(src: &str) -> Vec<Token>  {
    let mut lexer = Lexer::new(src);
    let mut tokens: Vec<Token> = vec![];

    while tokens.len() == 0 || tokens.last().unwrap().kind() != TokenKind::EOF {
        tokens.push(lexer.next_token());
    }

    tokens
}*/