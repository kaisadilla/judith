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
    pub fn parse_top_level_node(&mut self) -> ParseAttempt<SyntaxNode> {
        match self.parse_item() {
            ParseAttempt::Ok(it) => return ParseAttempt::Ok(SyntaxNode::Item(it)),
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            _ => {},
        };

        match self.parse_stmt() {
            ParseAttempt::Ok(stmt) => return ParseAttempt::Ok(SyntaxNode::Stmt(stmt)),
            ParseAttempt::Err(err) => return self.register_err_node(err),
            _ => {},
        };

        ParseAttempt::None // TODO: Should this be err? How do we detect when there's still tokens, but don't form a top level node?
    }

    // region Parse items
    pub fn parse_item(&mut self) -> ParseAttempt<Item> {
        match self.parse_func_def() {
            ParseAttempt::Ok(func_def) => return ParseAttempt::Ok(Item::FuncDef(func_def)),
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            _ => {},
        };

        ParseAttempt::None
    }

    pub fn parse_func_def(&mut self) -> ParseAttempt<FuncDef> {
        let func_tok = match self.try_consume(TokenKind::KwFunc) {
            Some(tok) => tok,
            None => return ParseAttempt::None,
        };

        let name = match self.parse_simple_identifier() {
            ParseAttempt::Ok(name) => name,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Err(
                compiler_messages::Parser::identifier_expected(self.now())
            ),
        };

        let param_list = match self.parse_parameter_list() {
            ParseAttempt::Ok(param_list) => param_list,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Err(
                compiler_messages::Parser::parameter_list_expected(self.now())
            )
        };

        let arrow_tok = self.try_consume(TokenKind::MinusArrow);
        let return_type = match &arrow_tok {
            Some(tok) => {
                match self.parse_type() {
                    ParseAttempt::Ok(ty) => Some(ty),
                    ParseAttempt::Err(err) => return ParseAttempt::Err(err),
                    ParseAttempt::None => return ParseAttempt::Err(
                        compiler_messages::Parser::type_expected(self.now())
                    )
                }
            }
            None => None,
        };

        let body = match self.parse_body(None) {
            ParseAttempt::Ok(body) => body,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Err(
                compiler_messages::Parser::body_expected(self.now())
            )
        };

        ParseAttempt::Ok(
            SyntaxFactory::func_def(func_tok, name, param_list, arrow_tok, return_type, body)
        )
    }
    // endregion Parse items

    // region Parse bodies
    pub fn parse_body(&mut self, opening_token: Option<TokenKind>) -> ParseAttempt<Body> {
        // Arrow must be tried first, since block statement may not have an opening token (and thus
        // would report an error trying to read an arrow).
        match self.parse_arrow_body() {
            ParseAttempt::Ok(arrow_body) => return ParseAttempt::Ok(Body::Arrow(arrow_body)),
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            _ => {}
        };

        match self.parse_block_body(opening_token) {
            ParseAttempt::Ok(block) => return ParseAttempt::Ok(Body::Block(block)),
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            _ => {}
        };

        ParseAttempt::None
    }

    pub fn parse_block_body(&mut self, opening_token: Option<TokenKind>) -> ParseAttempt<BlockBody> {
        let opening_token = if let Some(tok_kind) = opening_token {
            match self.try_consume(tok_kind) {
                Some(tok) => Some(tok),
                None => return ParseAttempt::None,
            }
        }
        else {
            None
        };

        let mut nodes: Vec<SyntaxNode> = Vec::new();
        let closing_token: Option<Token>;
        loop {
            if self.is_at_end() {
                return ParseAttempt::Err(
                    compiler_messages::Parser::end_expected(self.now())
                )
            };
            if let Some(tok) = self.try_consume(TokenKind::KwEnd) {
                closing_token = Some(tok);
                break;
            }
            if self.check_many(&[TokenKind::KwEnd, TokenKind::KwElse, TokenKind::KwElsif]) {
                closing_token = None;
                break;
            }

            let node = match self.parse_top_level_node() {
                ParseAttempt::Ok(node) => node,
                ParseAttempt::Err(err) => return ParseAttempt::Err(err),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::unexpected_token(self.now())
                ),
            };

            nodes.push(node);
        }

        ParseAttempt::Ok(SyntaxFactory::block_body(opening_token, nodes, closing_token))
    }

    pub fn parse_arrow_body(&mut self) -> ParseAttempt<ArrowBody> {
        let arrow_tok = match self.try_consume(TokenKind::EqualArrow) {
            Some(tok) => tok,
            None => return ParseAttempt::None,
        };

        let expr = match self.parse_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Err(
                compiler_messages::Parser::expression_expected(self.now())
            ),
        };

        ParseAttempt::Ok(SyntaxFactory::arrow_body(arrow_tok, expr))
    }
    // endregion

    // region Parse statements
    pub fn parse_stmt(&mut self) -> ParseAttempt<Stmt> {
        match self.parse_local_decl_stmt() {
            ParseAttempt::Ok(stmt) => return ParseAttempt::Ok(Stmt::LocalDecl(stmt)),
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            _ => {}
        }
        match self.parse_expr_stmt() {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(Stmt::Expr(expr)),
            ParseAttempt::Err(err) => return self.register_err_stmt(err),
            _ => {}
        };

        ParseAttempt::None
    }

    pub fn parse_expr_stmt(&mut self) -> ParseAttempt<ExprStmt> {
        let expr = match self.parse_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::None,
        };

        ParseAttempt::Ok(SyntaxFactory::expr_stmt(expr))
    }

    pub fn parse_local_decl_stmt(&mut self) -> ParseAttempt<LocalDeclStmt> {
        let let_tok = match self.try_consume(TokenKind::KwLet) {
            Some(tok) => tok,
            None => return ParseAttempt::None,
        };

        let declarator = match self.parse_local_declarator() {
            ParseAttempt::Ok(declarator) => declarator,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Err(
                compiler_messages::Parser::variable_declarator_expected(self.now())
            ),
        };

        let init = match self.parse_equals_value_clause(false) {
            ParseAttempt::Ok(init) => Some(init),
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => None,
        };

        ParseAttempt::Ok(SyntaxFactory::local_decl_stmt(let_tok, declarator, init))
    }
    // endregion Parse statements

    // region Parse expressions
    // expr ::= if_expr | loop_expr | while_expr | assignment_expr
    pub fn parse_expr(&mut self) -> ParseAttempt<Expr> {
        match self.parse_if_expr(TokenKind::KwIf) {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(Expr::If(Box::from(expr))),
            ParseAttempt::Err(err) => return self.register_err_expr(err),
            _ => {}
        };
        match self.parse_loop_expr() {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(Expr::Loop(Box::from(expr))),
            ParseAttempt::Err(err) => return self.register_err_expr(err),
            _ => {}
        };
        match self.parse_while_expr() {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(Expr::While(Box::from(expr))),
            ParseAttempt::Err(err) => return self.register_err_expr(err),
            _ => {}
        };
        match self.parse_assignment_expr() {
            ParseAttempt::Ok(expr) => return ParseAttempt::Ok(expr),
            ParseAttempt::Err(err) => return self.register_err_expr(err),
            _ => {},
        };

        ParseAttempt::None
    }

    // if_expr ::=
    pub fn parse_if_expr(&mut self, if_token_kind: TokenKind) -> ParseAttempt<IfExpr> {
        let if_tok = match self.try_consume(if_token_kind) {
            Some(tok) => tok,
            None => return ParseAttempt::None,
        };

        let test = match self.parse_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Err(
                compiler_messages::Parser::expression_expected(self.now())
            ),
        };

        let consequent = match self.parse_body(Some(TokenKind::KwThen)) {
            ParseAttempt::Ok(body) => body,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Err(
                compiler_messages::Parser::body_expected(self.now())
            )
        };

        // We first check if there's an "elsif" block. In this case, we don't consume the token, as
        // it will become the opening token for the subsequent (els)if expression.
        if self.check(TokenKind::KwElsif) {
            let alternate = match self.parse_if_expr(TokenKind::KwElsif) {
                ParseAttempt::Ok(elsif) => elsif,
                ParseAttempt::Err(err) => return ParseAttempt::Err(err),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::elsif_body_expected(self.now())
                )
            };

            ParseAttempt::Ok(SyntaxFactory::if_else_expr(
                if_tok,
                test,
                consequent,
                None,
                Body::Expr(SyntaxFactory::expr_body(
                    Expr::If(Box::from(alternate))
                ))
            ))
        }
        // Then we check if there's an "else" block. In this case, we consume the token, as we'll
        // use it here.
        else if let Some(else_tok) = self.try_consume(TokenKind::KwElse) {
            let alternate = match self.parse_body(None) {
                ParseAttempt::Ok(body) => body,
                ParseAttempt::Err(err) => return ParseAttempt::Err(err),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::body_expected(self.now())
                )
            };

            ParseAttempt::Ok(
                SyntaxFactory::if_else_expr(if_tok, test, consequent, Some(else_tok), alternate)
            )
        }
        // If there isn't either an "elsif" nor an "else", we build an if expression without alternate.
        else {
            ParseAttempt::Ok(
                SyntaxFactory::if_expr(if_tok, test, consequent)
            )
        }
    }

    // loop_expr ::=
    pub fn parse_loop_expr(&mut self) -> ParseAttempt<LoopExpr> {
        let loop_tok = match self.try_consume(TokenKind::KwLoop) {
            Some(tok) => tok,
            None => return ParseAttempt::None,
        };

        let body = match self.parse_body(None) {
            ParseAttempt::Ok(body) => body,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Err(
                compiler_messages::Parser::body_expected(self.now())
            )
        };

        ParseAttempt::Ok(SyntaxFactory::loop_expr(loop_tok, body))
    }

    // while_expr ::=
    pub fn parse_while_expr(&mut self) -> ParseAttempt<WhileExpr> {
        let while_expr = match self.try_consume(TokenKind::KwWhile) {
            Some(tok) => tok,
            None => return ParseAttempt::None,
        };

        let test = match self.parse_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Err(
                compiler_messages::Parser::expression_expected(self.now())
            ),
        };

        let body = match self.parse_body(Some(TokenKind::KwDo)) {
            ParseAttempt::Ok(body) => body,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Err(
                compiler_messages::Parser::body_expected(self.now())
            )
        };

        ParseAttempt::Ok(SyntaxFactory::while_expr(while_expr, test, body))
    }

    // region Parse cascading expressions
    // assignment_expr ::= or_logical_expr ( "=" or_logical_expr )?
    pub fn parse_assignment_expr(&mut self) -> ParseAttempt<Expr> {
        let left = match self.parse_or_logical_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::None,
        };

        let op = match self.parse_operator(&[TokenKind::Equal]) {
            ParseAttempt::Ok(op) => op,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Ok(left),
        };

        let right = match self.parse_or_logical_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Err(
                compiler_messages::Parser::expression_expected(self.now())
            ),
        };

        ParseAttempt::Ok(Expr::Assignment(
            Box::from(SyntaxFactory::assignment_expr(left, op, right))
        ))
    }

    // or_logical_expr ::= and_logical_expr ( "or" and_logical_expr )*
    pub fn parse_or_logical_expr(&mut self) -> ParseAttempt<Expr> {
        let mut left = match self.parse_and_logical_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::None,
        };

        while let ParseAttempt::Ok(op) = self.parse_operator(&[
            TokenKind::KwOr,
        ]) {
            let right = match self.parse_and_logical_expr() {
                ParseAttempt::Ok(expr) => expr,
                ParseAttempt::Err(err) => return ParseAttempt::Err(err),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::expression_expected(self.now())
                ),
            };

            left = Expr::Binary(
                Box::from(SyntaxFactory::binary_expr(left, op, right))
            );
        }

        ParseAttempt::Ok(left)
    }

    // and_logical_expr ::= parse_bool_expr ( "and" parse_bool_expr )*
    pub fn parse_and_logical_expr(&mut self) -> ParseAttempt<Expr> {
        let mut left = match self.parse_bool_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::None,
        };

        while let ParseAttempt::Ok(op) = self.parse_operator(&[
            TokenKind::KwAnd,
        ]) {
            let right = match self.parse_bool_expr() {
                ParseAttempt::Ok(expr) => expr,
                ParseAttempt::Err(err) => return ParseAttempt::Err(err),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::expression_expected(self.now())
                ),
            };

            left = Expr::Binary(
                Box::from(SyntaxFactory::binary_expr(left, op, right))
            );
        }

        ParseAttempt::Ok(left)
    }

    // bool_expr ::= add_expr ( ( "==" | "!=" | "~=" | "!~" | "===" | "!==" | "<" | "<=" | ">" | ">=" ) add_expr )*
    pub fn parse_bool_expr(&mut self) -> ParseAttempt<Expr> {
        let mut left = match self.parse_add_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::None,
        };

        while let ParseAttempt::Ok(op) = self.parse_operator(&[
            TokenKind::EqualEqual,
            TokenKind::BangEqual,
            TokenKind::TildeTilde,
            TokenKind::BangTilde,
            TokenKind::EqualEqualEqual,
            TokenKind::BangEqualEqual,
            TokenKind::Less,
            TokenKind::LessEqual,
            TokenKind::Greater,
            TokenKind::GreaterEqual,
        ]) {
            let right = match self.parse_add_expr() {
                ParseAttempt::Ok(expr) => expr,
                ParseAttempt::Err(err) => return ParseAttempt::Err(err),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::expression_expected(self.now())
                ),
            };

            left = Expr::Binary(
                Box::from(SyntaxFactory::binary_expr(left, op, right))
            );
        }

        ParseAttempt::Ok(left)
    }

    // add_expr ::= mult_expr ( ( "+" | "-" ) mult_expr )*
    pub fn parse_add_expr(&mut self) -> ParseAttempt<Expr> {
        let mut left = match self.parse_mult_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::None,
        };

        while let ParseAttempt::Ok(op) = self.parse_operator(
            &[TokenKind::Plus, TokenKind::Minus]
        ) {
            let right = match self.parse_mult_expr() {
                ParseAttempt::Ok(expr) => expr,
                ParseAttempt::Err(err) => return ParseAttempt::Err(err),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::expression_expected(self.now())
                ),
            };

            left = Expr::Binary(
                Box::from(SyntaxFactory::binary_expr(left, op, right))
            );
        }

        ParseAttempt::Ok(left)
    }

    // mult_expr ::= left_unary-expr ( ( "*" | "/" ) left_unary_expr )*
    pub fn parse_mult_expr(&mut self) -> ParseAttempt<Expr> {
        let mut left = match self.parse_left_unary_expr() {
            ParseAttempt::Ok(expr) => expr,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::None,
        };

        while let ParseAttempt::Ok(op) = self.parse_operator(
            &[TokenKind::Asterisk, TokenKind::Slash]
        ) {
            let right = match self.parse_left_unary_expr() {
                ParseAttempt::Ok(expr) => expr,
                ParseAttempt::Err(err) => return ParseAttempt::Err(err),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::expression_expected(self.now())
                ),
            };

            left = Expr::Binary(
                Box::from(SyntaxFactory::binary_expr(left, op, right))
            );
        }

        ParseAttempt::Ok(left)
    }

    // left_unary_expr ::= ( ( "not" | "-" | "~" ) left_unary_expr ) | object_init_expr
    pub fn parse_left_unary_expr(&mut self) -> ParseAttempt<Expr> {
        if let ParseAttempt::Ok(op) = self.parse_operator(
            &[TokenKind::KwNot, TokenKind::Minus, TokenKind::Tilde]
        ) {
            let expr = match self.parse_left_unary_expr() {
                ParseAttempt::Ok(expr) => expr,
                ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::expression_expected(self.now())
                ),
            };

            ParseAttempt::Ok(Expr::LeftUnary(
                Box::from(SyntaxFactory::left_unary_expr(op, expr))
            ))
        }
        else {
            self.parse_object_init_expr()
        }
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
            ParseAttempt::None => return match provider {
                Some(expr) => ParseAttempt::Ok(expr),
                None => ParseAttempt::None,
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
    // endregion Parse cascading expressions

    // endregion Parse expressions

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

    pub fn parse_type_annotation(&mut self) -> ParseAttempt<TypeAnnotation> {
        let delimiter_tok = match self.try_consume(TokenKind::Colon) {
            Some(tok) => tok,
            None => return ParseAttempt::None,
        };

        let ty = match self.parse_type() {
            ParseAttempt::Ok(ty) => ty,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::Err(
                compiler_messages::Parser::type_expected(self.now())
            ),

        };

        ParseAttempt::Ok(SyntaxFactory::type_annotation(delimiter_tok, ty))
    }

    pub fn parse_operator(&mut self, kinds: &[TokenKind]) -> ParseAttempt<Operator> {
        let op_tok = self.try_consume_many(kinds);
        if op_tok.is_none() {
            return ParseAttempt::None;
        }

        ParseAttempt::Ok(SyntaxFactory::operator(op_tok.unwrap()))
    }

    // param_list ::= "(" ( param ( "," param )* ","? )? ")"
    pub fn parse_parameter_list(&mut self) -> ParseAttempt<ParameterList> {
        let left_paren = match self.try_consume(TokenKind::LeftParen) {
            Some(tok) => tok,
            None => return ParseAttempt::None,
        };

        let mut params: Vec<Parameter> = Vec::new();
        let mut comma_tokens: Vec<Token> = Vec::new();
        if self.check(TokenKind::RightParen) == false {
            loop {
                let param = match self.parse_parameter() {
                    ParseAttempt::Ok(param) => param,
                    ParseAttempt::Err(err) => return ParseAttempt::Err(err),
                    ParseAttempt::None => return ParseAttempt::Err(
                        compiler_messages::Parser::parameter_expected(self.now())
                    ),
                };
                params.push(param);

                // After the parameter, there may or may not be a comma. If there isn't a comma
                // then no more parameters can be found.
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
            };
        }

        let right_paren = match self.try_consume(TokenKind::RightParen) {
            Some(tok) => tok,
            None => return ParseAttempt::Err(compiler_messages::Parser::right_paren_expected(self.now())),
        };

        ParseAttempt::Ok(SyntaxFactory::parameter_list(left_paren, params, right_paren, comma_tokens))
    }

    // param ::= decl equals_value_clause?
    pub fn parse_parameter(&mut self) -> ParseAttempt<Parameter> {
        let decl = match self.parse_regular_local_declarator() {
            ParseAttempt::Ok(decl) => decl,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::None,
        };

        let default_val = match self.parse_equals_value_clause(false) {
            ParseAttempt::Ok(val) => Some(val),
            ParseAttempt::None => None,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
        };

        ParseAttempt::Ok(SyntaxFactory::parameter(decl, default_val))
    }

    // arg_list ::= "(" ( arg ( "," arg )* ","? )? ")"
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
            };
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

    pub fn parse_local_declarator(&mut self) -> ParseAttempt<PartialLocalDecl> {
        match self.parse_regular_local_declarator() {
            ParseAttempt::Ok(node) => return ParseAttempt::Ok(
                PartialLocalDecl::Regular(node)
            ),
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            _ => {}
        };

        // TODO: Destructure local declarator

        ParseAttempt::None
    }

    pub fn parse_regular_local_declarator(&mut self) -> ParseAttempt<RegularLocalDecl> {
        let ownership_tok = self.parse_ownership_token();

        let ident = match self.parse_simple_identifier() {
            ParseAttempt::Ok(ident) => ident,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::None,
        };

        let type_annot = match self.parse_type_annotation() {
            ParseAttempt::Ok(type_annot) => Some(type_annot),
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => None,
        };

        ParseAttempt::Ok(SyntaxFactory::regular_local_decl(
            SyntaxFactory::local_declarator(ownership_tok, ident, type_annot)
        ))
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

    pub fn parse_type(&mut self) -> ParseAttempt<TypeNode> {
        let ownership_tok = self.parse_ownership_token();

        match self.parse_sum_type() {
            ParseAttempt::Ok(ty) => ParseAttempt::Ok(SyntaxFactory::type_node(
                ownership_tok,
                ty.ty,
                ty.nullable_token,
            )),
            ParseAttempt::Err(msg) => ParseAttempt::Err(msg),
            ParseAttempt::None => ParseAttempt::None,
        }
    }

    // sum_type ::= product_type ( "|" product_type )*
    pub fn parse_sum_type(&mut self) -> ParseAttempt<TypeNode> {
        let ty = match self.parse_product_type() {
            ParseAttempt::Ok(ty) => ty,
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            ParseAttempt::None => return ParseAttempt::None,
        };

        if self.check(TokenKind::Pipe) == false {
            return ParseAttempt::Ok(ty);
        }

        let mut member_types = vec![ty];
        let mut or_tokens: Vec<Token> = Vec::new();
        loop {
            match self.try_consume(TokenKind::Pipe) {
                Some(tok) => or_tokens.push(tok),
                None => break,
            };

            match self.parse_product_type() {
                ParseAttempt::Ok(ty) => member_types.push(ty),
                ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::type_expected(self.now())
                ),
            };
        };

        ParseAttempt::Ok(SyntaxFactory::type_node(
            None,
            PartialType::Sum(Box::from(
                SyntaxFactory::sum_type(member_types, or_tokens)
            )),
            None,
        ))
    }

    // product_type ::= raw_array_type ( "&" raw_array_type )*
    pub fn parse_product_type(&mut self) -> ParseAttempt<TypeNode> {
        let ty = match self.parse_raw_array_type() {
            ParseAttempt::Ok(ty) => ty,
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            ParseAttempt::None => return ParseAttempt::None,
        };

        if self.check(TokenKind::Ampersand) == false {
            return ParseAttempt::Ok(ty);
        }

        let mut member_types = vec![ty];
        let mut and_tokens: Vec<Token> = Vec::new();
        loop {
            match self.try_consume(TokenKind::Ampersand) {
                Some(tok) => and_tokens.push(tok),
                None => break,
            };

            match self.parse_raw_array_type() {
                ParseAttempt::Ok(ty) => member_types.push(ty),
                ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::type_expected(self.now())
                ),
            };
        };

        ParseAttempt::Ok(SyntaxFactory::type_node(
            None,
            PartialType::Product(Box::from(
                SyntaxFactory::product_type(member_types, and_tokens)
            )),
            None,
        ))
    }

    // raw_array_type ::= primary_type ( "[" expr "]" )*
    pub fn parse_raw_array_type(&mut self) -> ParseAttempt<TypeNode> {
        let ty = match self.parse_primary_type() {
            ParseAttempt::Ok(ty) => ty,
            ParseAttempt::Err(err) => return ParseAttempt::Err(err),
            ParseAttempt::None => return ParseAttempt::None,
        };

        let nullable_tok = self.try_consume(TokenKind::QuestionMark);

        let mut ty= SyntaxFactory::type_node(None, ty, nullable_tok);

        loop {
            let left_sq_bracket = match self.try_consume(TokenKind::LeftSquareBracket) {
                Some(tok) => tok,
                None => return ParseAttempt::Ok(ty),
            };

            let len = match self.parse_expr() {
                ParseAttempt::Ok(expr) => expr,
                ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::expression_expected(self.now())
                ),
            };

            let right_sq_bracket = match self.try_consume(TokenKind::RightSquareBracket) {
                Some(tok) => tok,
                None => return ParseAttempt::Err(
                    compiler_messages::Parser::right_square_bracket_expected(self.now())
                ),
            };

            let nullable_tok = self.try_consume(TokenKind::QuestionMark);

            ty = SyntaxFactory::type_node(
                None,
                PartialType::RawArray(Box::from(
                    SyntaxFactory::raw_array_type(ty, left_sq_bracket, len, right_sq_bracket)
                )),
                nullable_tok,
            );
        }
    }

    // primary_type ::= identifier_type | function_type | tuple_array_type | literal_type
    pub fn parse_primary_type(&mut self) -> ParseAttempt<PartialType> {
        match self.parse_function_type() {
            ParseAttempt::Ok(ty) => return ParseAttempt::Ok(ty),
            ParseAttempt::Err(err) => return self.register_err_type(err),
            _ => {}
        };
        match self.parse_tuple_array_type() {
            ParseAttempt::Ok(ty) => return ParseAttempt::Ok(PartialType::TupleArray(Box::from(ty))),
            ParseAttempt::Err(err) => return self.register_err_type(err),
            _ => {}
        };
        match self.parse_literal_type() {
            ParseAttempt::Ok(ty) => return ParseAttempt::Ok(PartialType::Literal(ty)),
            ParseAttempt::Err(err) => return self.register_err_type(err),
            _ => {}
        };
        match self.parse_identifier_type() {
            ParseAttempt::Ok(ty) => return ParseAttempt::Ok(PartialType::Identifier(ty)),
            ParseAttempt::Err(err) => return self.register_err_type(err),
            _ => {}
        };

        ParseAttempt::None
    }

    // identifier_type ::= identifier
    pub fn parse_identifier_type(&mut self) -> ParseAttempt<IdentifierType> {
        let identifier = match self.parse_qualified_identifier() {
            ParseAttempt::Ok(id) => id,
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            ParseAttempt::None => return ParseAttempt::None,
        };

        ParseAttempt::Ok(SyntaxFactory::identifier_type(identifier))
    }

    // function_type ::= ( "s" | "S" | "sS" )? "!"? "(" type ( "," type )* ")" "->" type
    //                 | "(" type ")" ; group_type
    pub fn parse_function_type(&mut self) -> ParseAttempt<PartialType> {
        // We may be parsing a function type or a group type. More on that later.

        // A function type may start with the ss token ('s' for Send, 'S' for Sync, or 'sS' for both).
        let ss_tok = match self.peek() {
            Some(tok) if tok.base().lexeme == "s" => Some(tok),
            Some(tok) if tok.base().lexeme == "S" => Some(tok),
            Some(tok) if tok.base().lexeme == "sS" => Some(tok),
            _ => None,
        };

        // If we encountered an ss token, we move forwards, consuming it.
        if ss_tok.is_some() {
            self.advance();
        }

        // A function may then contain '!' to indicate it can return exceptions.
        let except_tok = self.try_consume(TokenKind::Bang);

        // Then we open parentheses to enumerate parameter types.
        let left_paren = match self.try_consume(TokenKind::LeftParen) {
            Some(tok) => tok,
            None => return if ss_tok.is_none() && except_tok.is_none() {
                ParseAttempt::None
            }
            else {
                ParseAttempt::Err(compiler_messages::Parser::parameter_type_list_expected(self.now()))
            },
        };

        let mut types: Vec<TypeNode> = Vec::new();
        let mut comma_tokens: Vec<Token> = Vec::new();
        if self.check(TokenKind::RightParen) == false {
            loop {
                let ty = match self.parse_type() {
                    ParseAttempt::Ok(ty) => ty,
                    ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
                    ParseAttempt::None => return ParseAttempt::Err(
                        compiler_messages::Parser::type_expected(self.now())
                    ),
                };
                types.push(ty);

                // After the type, there may or may not be a comma. If there isn't a comma, then
                // no more types can be found.
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
        };

        let right_paren = match self.try_consume(TokenKind::RightParen) {
            Some(tok) => tok,
            None => return ParseAttempt::Err(compiler_messages::Parser::right_paren_expected(self.now()))
        };

        // Now, there's 3 possible scenarios: if we encounter '->', this is a function type. If we
        // don't; then if the type doesn't have an ss token nor an exception token, and there's exactly
        // one parameter type, then we have actually parsed a grouping type. In the case there's no
        // '->' but the conditions for a group type aren't fulfilled, we are parsing an incorrect
        // function or group type.

        // We are parsing a function.
        if let Some(arrow_tok) = self.try_consume(TokenKind::EqualArrow) {
            let return_type = match self.parse_type() {
                ParseAttempt::Ok(ty) => ty,
                ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
                ParseAttempt::None => return ParseAttempt::Err(
                    compiler_messages::Parser::type_expected(self.now())
                ),
            };

            ParseAttempt::Ok(PartialType::Function(Box::from(
                SyntaxFactory::function_type(
                    ss_tok,
                    except_tok,
                    left_paren,
                    types,
                    right_paren,
                    comma_tokens,
                    arrow_tok,
                    return_type
                )
            )))
        }
        // We are parsing a group type.
        else if ss_tok.is_none() && except_tok.is_none() {
            // The group type is empty, which isn't allowed.
            if types.len() == 0 {
                ParseAttempt::Err(compiler_messages::Parser::type_expected(self.now()))
            }
            // The group type has exactly one type, which is allowed.
            else if types.len() == 1 {
                ParseAttempt::Ok(PartialType::Group(Box::from(
                    SyntaxFactory::group_type(left_paren, types.remove(0), right_paren)
                )))
            }
            // The group type has many types separated by commas, which isn't allowed.
            else {
                ParseAttempt::Err(compiler_messages::Parser::right_paren_expected(self.now()))
            }
        }
        // We are parsing an incorrect function type.
        else {
            ParseAttempt::Err(compiler_messages::Parser::return_type_expected(self.now()))
        }
    }

    // tuple_array_type ::= "[" type ( "," type )* "]"
    pub fn parse_tuple_array_type(&mut self) -> ParseAttempt<TupleArrayType> {
        let left_sq_bracket = match self.try_consume(TokenKind::LeftSquareBracket) {
            Some(tok) => tok,
            None => return ParseAttempt::None,
        };

        let mut types: Vec<TypeNode> = Vec::new();
        let mut comma_tokens: Vec<Token> = Vec::new();
        if self.check(TokenKind::RightSquareBracket) == false {
            loop {
                let ty = match self.parse_type() {
                    ParseAttempt::Ok(ty) => ty,
                    ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
                    ParseAttempt::None => return ParseAttempt::Err(
                        compiler_messages::Parser::type_expected(self.now())
                    ),
                };
                types.push(ty);

                // After the type, there may or may not be a comma. If there isn't a comma, then
                // no more types can be found.
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
        };

        let right_sq_bracket = match self.try_consume(TokenKind::RightSquareBracket) {
            Some(tok) => tok,
            None => return ParseAttempt::Err(
                compiler_messages::Parser::right_square_bracket_expected(self.now())
            )
        };

        ParseAttempt::Ok(
            SyntaxFactory::tuple_array_type(left_sq_bracket, types, right_sq_bracket, comma_tokens)
        )
    }

    // literal_type ::= literal
    pub fn parse_literal_type(&mut self) -> ParseAttempt<LiteralType> {
        let lit = match self.parse_literal() {
            ParseAttempt::Ok(lit) => lit,
            ParseAttempt::Err(msg) => return ParseAttempt::Err(msg),
            ParseAttempt::None => return ParseAttempt::None,
        };

        ParseAttempt::Ok(SyntaxFactory::literal_type(lit))
    }
    // endregion Types

    fn parse_ownership_token(&mut self) -> Option<Token> {
        if let Some(tok) = self.try_consume(TokenKind::KwFinal) {
            Some(tok)
        }
        else if let Some(tok) = self.try_consume(TokenKind::KwMut) {
            Some(tok)
        }
        else if let Some(tok) = self.try_consume(TokenKind::KwIn) {
            Some(tok)
        }
        else if let Some(tok) = self.try_consume(TokenKind::KwSh) {
            Some(tok)
        }
        else if let Some(tok) = self.try_consume(TokenKind::KwRef) {
            Some(tok)
        }
        else {
            None
        }
    }

    // endregion Parse methods

    /// Marks the lexer as containing errors and adds the message to the container.
    fn error(&mut self, msg: CompilerMessage) {
        self.has_errors = true;
        self.messages.add(msg);
    }

    fn register_err_node(&mut self, err: CompilerMessage) -> ParseAttempt<SyntaxNode> {
        self.messages.add(err);

        ParseAttempt::Ok(SyntaxNode::Error(SyntaxFactory::error_node()))
    }

    fn register_err_stmt(&mut self, err: CompilerMessage) -> ParseAttempt<Stmt> {
        self.messages.add(err);

        ParseAttempt::Ok(Stmt::Error(SyntaxFactory::error_node()))
    }

    fn register_err_expr(&mut self, err: CompilerMessage) -> ParseAttempt<Expr> {
        self.messages.add(err);

        ParseAttempt::Ok(Expr::Error(SyntaxFactory::error_node()))
    }

    fn register_err_type(&mut self, err: CompilerMessage) -> ParseAttempt<PartialType> {
        self.messages.add(err);

        ParseAttempt::Ok(PartialType::Error(SyntaxFactory::error_node()))
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

#[cfg(test)]
mod tests {
    use crate::judith::lexical::lexer::tokenize;
    use super::*;

    #[test]
    fn valid_identifier_type() {
        println!("== Testing identifier type ==");

        println!("Testing 'Num'.");
        let lexer_res = tokenize("Num");
        let mut parser = Parser::new(&lexer_res.tokens);
        let ParseAttempt::Ok(node) = parser.parse_type() else { panic!("Parse failed.") };

        assert!(matches!(&node.ty, PartialType::Identifier(_)));
        let PartialType::Identifier(ident) = &node.ty else { panic!("???") };

        assert!(matches!(&ident.name, Identifier::Simple(_)));
        let Identifier::Simple(ident) = &ident.name else { panic!("???") };

        assert_eq!(ident.name, "Num");
        assert_eq!(node.is_nullable, false);
        assert_eq!(node.ownership_kind, OwnershipKind::None);

        println!("Testing 'mut String?'.");
        let lexer_res = tokenize("mut String?");
        let mut parser = Parser::new(&lexer_res.tokens);
        let ParseAttempt::Ok(node) = parser.parse_type() else { panic!("Parse failed.") };

        assert!(matches!(&node.ty, PartialType::Identifier(_)));
        let PartialType::Identifier(ident) = &node.ty else { panic!("???") };

        assert!(matches!(&ident.name, Identifier::Simple(_)));
        let Identifier::Simple(ident) = &ident.name else { panic!("???") };

        assert_eq!(ident.name, "String");
        assert_eq!(node.is_nullable, true);
        assert_eq!(node.ownership_kind, OwnershipKind::Mutable);
    }

    #[test]
    fn valid_local_decl_stmt() {
        println!("== Testing local declaration statement ==");

        println!("Testing 'let n: String = \"Kevin\"'.");
        let lexer_res = tokenize("let n: String = \"Kevin\"");
        let mut parser = Parser::new(&lexer_res.tokens);
        let ParseAttempt::Ok(node) = parser.parse_local_decl_stmt() else { panic!("Parse failed.") };

        assert!(matches!(&node.decl, PartialLocalDecl::Regular(_)));
        let PartialLocalDecl::Regular(decl) = &node.decl else { panic!("???") };

        assert!(matches!(&decl.declarator.ownership_kind, OwnershipKind::None));
        assert_eq!(&decl.declarator.name.name, "n");
        assert!(&decl.declarator.type_annotation.is_some());
        assert!(node.initializer.is_some());

        println!("Testing 'let mut score = 42'.");
        let lexer_res = tokenize("let mut score = 42");
        let mut parser = Parser::new(&lexer_res.tokens);
        let ParseAttempt::Ok(node) = parser.parse_local_decl_stmt() else { panic!("Parse failed.") };

        assert!(matches!(&node.decl, PartialLocalDecl::Regular(_)));
        let PartialLocalDecl::Regular(decl) = &node.decl else { panic!("???") };

        assert!(matches!(&decl.declarator.ownership_kind, OwnershipKind::Mutable));
        assert_eq!(&decl.declarator.name.name, "score");
        assert!(&decl.declarator.type_annotation.is_none());
        assert!(node.initializer.is_some());

        println!("Testing 'let sh res");
        let lexer_res = tokenize("let sh res");
        let mut parser = Parser::new(&lexer_res.tokens);
        let ParseAttempt::Ok(node) = parser.parse_local_decl_stmt() else { panic!("Parse failed.") };

        assert!(matches!(&node.decl, PartialLocalDecl::Regular(_)));
        let PartialLocalDecl::Regular(decl) = &node.decl else { panic!("???") };

        assert!(matches!(&decl.declarator.ownership_kind, OwnershipKind::Shared));
        assert_eq!(&decl.declarator.name.name, "res");
        assert!(&decl.declarator.type_annotation.is_none());
        assert!(node.initializer.is_none());
    }
}