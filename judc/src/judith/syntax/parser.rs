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
    // node ::= expr
    pub fn parse_top_level_node (&mut self) -> ParseAttempt<SyntaxNode> {
        match self.parse_expr() {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(SyntaxNode::Expr(expr)),
            ParseAttempt::Err(err) => return self.register_err_node(err),
            _ => {},
        };

        ParseAttempt::None
    }

    // expr ::= object_init_expr
    pub fn parse_expr (&mut self) -> ParseAttempt<Expr> {
        match self.parse_object_init_expr() {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(expr),
            ParseAttempt::Err(err) => return self.register_err_expr(err),
            _ => {},
        };

        ParseAttempt::None
    }

    // object_init_expr ::= call_expr obj_initialization?
    pub fn parse_object_init_expr(&mut self) -> ParseAttempt<Expr> {
        // An object initialization may not have any provider (for anonymous structs, or structs whose
        // type can be inferred). For this reason, we can't discard an object initialization even if
        // the provider is not found.
        let provider = match self.parse_call_expr() {
            ParseAttempt::Ok(expr) => Some(expr),
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            ParseAttempt::None => None,
        };

        // If there's no initializer block, then whether this parse failed or succeeded depends on
        // whether there's a provider.
        let obj_init = match self.parse_object_initializer() {
            ParseAttempt::Ok(obj_init) => obj_init,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => match provider {
                Some(expr) => return ParseAttempt::Ok(expr),
                None => return ParseAttempt::None,
            }
        };

        ParseAttempt::Ok(Expr::ObjectInit(
            Box::from(SyntaxFactory::object_init_expr(provider, obj_init))
        ))
    }

    // call_expr ::= access_expr arg_list*
    pub fn parse_call_expr(&mut self) -> ParseAttempt<Expr> {
        let callee = match self.parse_access_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::None,
        };

        let arg_list = match self.parse_argument_list() {
            ParseAttempt::Ok(arg_list) => arg_list,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Ok(callee),
        };
        
        ParseAttempt::Ok(Expr::Call(
            Box::from(SyntaxFactory::call_expr(callee, arg_list))
        ))
    }

    // access_expr ::= primary_expr ( "." primary_expr )*
    pub fn parse_access_expr(&mut self) -> ParseAttempt<Expr> {
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

    // primary_expr ::= group_expr | identifier_expr | literal_expr
    pub fn parse_primary(&mut self) -> ParseAttempt<Expr> {
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

    // group_expr ::= "(" expr ")"
    pub fn parse_group_expr(&mut self) -> ParseAttempt<Expr> {
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

    // identifier_expr ::= qualified_identifier
    pub fn parse_identifier_expr(&mut self) -> ParseAttempt<Expr> {
        let id = match self.parse_qualified_identifier() {
            ParseAttempt::Ok(id) => id,
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            ParseAttempt::None => return ParseAttempt::None,
        };

        ParseAttempt::Ok(Expr::Identifier(
            Box::from(SyntaxFactory::identifier_expr(id))
        ))
    }

    // literal_expr ::= literal
    pub fn parse_literal_expr(&mut self) -> ParseAttempt<Expr> {
        match self.parse_literal() {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(Expr::Literal(
                Box::from(SyntaxFactory::literal_expr(expr))
            )),
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            _ => {}
        }

        ParseAttempt::None
    }

    // region Fragments
    // identifier ::= IDENTIFIER
    pub fn parse_simple_identifier(&mut self) -> ParseAttempt<SimpleIdentifier> {
        let id_tok = match self.try_consume(TokenKind::Identifier) {
            Some(id) => id,
            None => return ParseAttempt::None,
        };

        ParseAttempt::Ok(SyntaxFactory::simple_identifier(id_tok))
    }

    // qualified_identifier ::= IDENTIFIER ( "::" IDENTIFIER )*
    pub fn parse_qualified_identifier(&mut self) -> ParseAttempt<Identifier> {
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

    // literal ::= NUMBER | STRING | CHAR | REGEX | "true" | "false" | "null" | "undefined"
    pub fn parse_literal(&mut self) -> ParseAttempt<Literal> {
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

    pub fn parse_equals_value_clause(
        &mut self, allow_multiple: bool
    ) -> ParseAttempt<EqualsValueClause> {
        let equal_tok = match self.try_consume(TokenKind::Equal) {
            Some(tok) => tok,
            None => return ParseAttempt::None,
        };

        let mut expressions: Vec<Expr> = Vec::new();
        let mut comma_tokens: Vec<Token> = Vec::new();
        loop {
            let tok = self.now();
            let expr = match self.parse_expr() {
                ParseAttempt::Ok(expr) => expr,
                ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::expression_expected(tok)
                ),
            };

            expressions.push(expr);

            if allow_multiple == false {
                break;
            }

            if let Some(tok) = self.try_consume(TokenKind::Comma) {
                comma_tokens.push(tok);
            }
            else {
                break;
            }
        }

        ParseAttempt::Ok(SyntaxFactory::equals_value_clause(equal_tok, expressions, comma_tokens))
    }

    pub fn parse_operator(&mut self, kinds: &[TokenKind]) -> ParseAttempt<Operator> {
        let op_tok = self.try_consume_many(kinds);
        if op_tok.is_none() {
            return ParseAttempt::None;
        }

        ParseAttempt::Ok(SyntaxFactory::operator(op_tok.unwrap()))
    }

    // arg_list ::= "(" ( arg ( "," arg )* )? ")"
    pub fn parse_argument_list(&mut self) -> ParseAttempt<ArgumentList> {
        let left_paren = match self.try_consume(TokenKind::LeftParen) {
            Some(tok) => tok,
            None => return ParseAttempt::None,
        };

        let mut args: Vec<Argument> = Vec::new();
        let mut comma_tokens: Vec<Token> = Vec::new();
        if self.check(TokenKind::RightParen) == false {
            loop {
                let tok = self.now();
                let arg = match self.parse_argument() {
                    ParseAttempt::Ok(arg) => arg,
                    ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
                    ParseAttempt::None => return ParseAttempt::Err(
                        compiler_messages::Parser::argument_expected(tok)
                    ),
                };
                args.push(arg);

                // After the argument, there may or may not be a comma. If there isn't a comma, then
                // no more arguments can be found.
                if let Some(tok) = self.try_consume(TokenKind::Comma) {
                    comma_tokens.push(tok);
                }
                else {
                    break;
                }

                // Trailing commas are allowed, so we may find a comma and still find the closing
                // parenthesis afterward.
                if self.check(TokenKind::RightParen) {
                    break;
                }
            }
        }

        let tok = self.now();
        let right_paren = match self.try_consume(TokenKind::RightParen) {
            Some(tok) => tok,
            None => return ParseAttempt::Err(compiler_messages::Parser::right_paren_expected(tok))
        };

        ParseAttempt::Ok(SyntaxFactory::argument_list(left_paren, args, right_paren, comma_tokens))
    }

    // arg ::= expr
    pub fn parse_argument(&mut self) -> ParseAttempt<Argument> {
        let expr = match self.parse_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            ParseAttempt::None => return ParseAttempt::None,
        };

        ParseAttempt::Ok(SyntaxFactory::argument(expr))
    }

    pub fn parse_field_init(&mut self) -> ParseAttempt<FieldInit> {
        let identifier = match self.parse_simple_identifier() {
            ParseAttempt::Ok(id) => id,
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            ParseAttempt::None => return ParseAttempt::None,
        };

        let tok = self.now();
        let initializer = match self.parse_equals_value_clause(false) {
            ParseAttempt::Ok(evc) => evc,
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            ParseAttempt::None => return ParseAttempt::Err(
                compiler_messages::Parser::field_must_be_initialized(tok)
            ),
        };

        ParseAttempt::Ok(SyntaxFactory::field_init(identifier, initializer))
    }

    pub fn parse_object_initializer(&mut self) -> ParseAttempt<ObjectInitializer> {
        let left_bracket = match self.try_consume(TokenKind::LeftCurlyBracket) {
            Some(tok) => tok,
            None => return ParseAttempt::None,
        };

        let mut field_inits: Vec<FieldInit> = Vec::new();
        let mut comma_tokens: Vec<Token> = Vec::new();
        if self.check(TokenKind::RightCurlyBracket) == false {
            loop {
                let tok = self.now();
                let init = match self.parse_field_init() {
                    ParseAttempt::Ok(arg) => arg,
                    ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
                    ParseAttempt::None => return ParseAttempt::Err(
                        compiler_messages::Parser::field_initialization_expected(tok)
                    ),
                };
                field_inits.push(init);

                // After the argument, there may or may not be a comma. If there isn't a comma, then
                // no more arguments can be found.
                if let Some(tok) = self.try_consume(TokenKind::Comma) {
                    comma_tokens.push(tok);
                }
                else {
                    break;
                }

                // Trailing commas are allowed, so we may find a comma and still find the closing
                // parenthesis afterward.
                if self.check(TokenKind::RightCurlyBracket) {
                    break;
                }
            }
        }

        let tok = self.now();
        let right_bracket = match self.try_consume(TokenKind::RightCurlyBracket) {
            Some(tok) => tok,
            None => return ParseAttempt::Err(compiler_messages::Parser::right_curly_bracket_expected(tok))
        };

        ParseAttempt::Ok(
            SyntaxFactory::object_initializer(left_bracket, field_inits, right_bracket, comma_tokens)
        )
    }
    // endregion Fragments

    // endregion Parse methods

    /// Marks the lexer as containing errors and adds the message to the container.
    fn error(&mut self, msg: CompilerMessage) {
        self.has_errors = true;
        self.messages.add(msg);
    }

    fn register_err_expr(&mut self, err: CompilerMessage) -> ParseAttempt<Expr> {
        self.messages.add(err);

        ParseAttempt::Ok(Expr::Error(SyntaxFactory::error_node()))
    }

    fn register_err_node(&mut self, err: CompilerMessage) -> ParseAttempt<SyntaxNode> {
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