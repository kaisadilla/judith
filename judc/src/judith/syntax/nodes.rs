use serde::Serialize;
use crate::judith::lexical::token::Token;
use crate::SourceSpan;

#[derive(Debug, Serialize)]
pub enum SyntaxNode {
    Expr(Expr),
}

#[derive(Debug)]
pub enum Expr {
    Assignment(Box<AssignmentExpr>),
    Binary(Box<BinaryExpr>),
    Group(Box<GroupExpr>),
    Literal(Box<LiteralExpr>),
}

#[derive(Debug, Serialize)]
pub struct AssignmentExpr {
    pub left: Expr,
    pub op: Operator,
    pub right: Expr,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct BinaryExpr {
    pub left: Expr,
    pub op: Operator,
    pub right: Expr,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct GroupExpr {
    pub expr: Expr,
    pub left_paren_token: Option<Token>,
    pub right_paren_token: Option<Token>,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct LiteralExpr {
    pub literal: Literal,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct Literal {
    pub source: String,
    pub rawToken: Option<Token>,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct Operator {

}

pub struct SyntaxFactory;

impl SyntaxFactory {
    pub fn group_expr (left_paren: Token, expr: Expr, right_paren: Token) -> GroupExpr {
        let start = left_paren.base().start;
        let end = right_paren.base().end;
        let line = left_paren.base().line;

        GroupExpr {
            expr,
            left_paren_token: Some(left_paren),
            right_paren_token: Some(right_paren),
            span: Some(SourceSpan { start, end, line }),
        }
    }

    pub fn literal_expr (literal: Literal) -> LiteralExpr {
        let span = literal.span.clone();

        LiteralExpr {
            literal,
            span,
        }
    }

    pub fn literal (tok: Token) -> Literal {
        let start = tok.base().start;
        let end = tok.base().end;
        let line = tok.base().line;

        Literal {
            source: tok.base().lexeme.clone(),
            rawToken: Some(tok),
            span: Some(SourceSpan { start, end, line }),
        }
    }
}