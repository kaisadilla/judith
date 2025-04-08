use crate::judith::lexical::token::Token;

#[derive(Debug)]
pub enum SyntaxNode {
    Expr(Expr),
}

#[derive(Debug)]
pub enum Expr {
    Assignment(Box<AssignmentExpr>),
    Binary(Box<BinaryExpr>),
    Literal(Box<LiteralExpr>)
}

#[derive(Debug)]
pub struct AssignmentExpr {
    pub left: Expr,
    pub op: Operator,
    pub right: Expr,
}

#[derive(Debug)]
pub struct BinaryExpr {
    pub left: Expr,
    pub op: Operator,
    pub right: Expr,
}

#[derive(Debug)]
pub struct LiteralExpr {
    pub source: String,
    pub rawToken: Option<Token>,
}

#[derive(Debug)]
pub struct Operator {

}