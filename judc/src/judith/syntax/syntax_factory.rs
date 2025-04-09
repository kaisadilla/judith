use crate::judith::lexical::token::{Token, TokenKind};
use crate::judith::syntax::nodes::*;
use crate::SourceSpan;

pub struct SyntaxFactory;

impl SyntaxFactory {
    // region Expressions
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

    pub fn identifier_expr (id: Identifier) -> IdentifierExpr {
        let span = id.span().clone();

        IdentifierExpr {
            identifier: id,
            span,
        }
    }

    pub fn literal_expr (literal: Literal) -> LiteralExpr {
        let span = literal.span.clone();

        LiteralExpr {
            literal,
            span,
        }
    }
    // endregion Expressions

    // region Fragments
    pub fn simple_identifier (tok: Token) -> SimpleIdentifier {
        const ESCAPE_CHAR: char = '\\';
        let start = tok.base().start;
        let end = tok.base().end;
        let line = tok.base().line;

        let mut is_escaped = false;
        let name = if tok.base().lexeme.starts_with(ESCAPE_CHAR) {
            is_escaped = true;
            tok.base().lexeme.chars().skip(1).collect::<String>()
        }
        else {
            tok.base().lexeme.clone()
        };

        SimpleIdentifier {
            is_meta_name: false,
            name,
            is_escaped,
            raw_token: Some(tok),
            span: Some(SourceSpan { start, end, line }),
        }
    }

    pub fn qualified_identifier (
        qualifier: Identifier, op: Operator, name: SimpleIdentifier
    ) -> QualifiedIdentifier {
        let start = qualifier.span().unwrap().start;
        let end = name.span.unwrap().end;
        let line = qualifier.span().unwrap().start;

        QualifiedIdentifier {
            is_meta_name: false,
            qualifier,
            operator: op,
            name,
            span: Some(SourceSpan { start, end, line }),
        }
    }

    pub fn literal (tok: Token) -> Literal {
        let start = tok.base().start;
        let end = tok.base().end;
        let line = tok.base().line;

        Literal {
            source: tok.base().lexeme.clone(),
            raw_token: Some(tok),
            span: Some(SourceSpan { start, end, line }),
        }
    }

    pub fn operator (tok: Token) -> Operator {
        let start = tok.base().start;
        let end = tok.base().end;
        let line = tok.base().line;

        let kind = match tok.kind() {
            TokenKind::Plus => OperatorKind::Add,
            TokenKind::Minus => OperatorKind::Subtract,
            TokenKind::Asterisk => OperatorKind::Multiply,
            TokenKind::Slash => OperatorKind::Divide,
            TokenKind::Tilde => OperatorKind::BitwiseNot,
            TokenKind::Equal => OperatorKind::Assignment,
            TokenKind::EqualEqual => OperatorKind::Equals,
            TokenKind::BangEqual => OperatorKind::NotEquals,
            TokenKind::TildeEqual => OperatorKind::Like,
            TokenKind::EqualEqualEqual => OperatorKind::ReferenceEquals,
            TokenKind::BangEqualEqual => OperatorKind::ReferenceNotEquals,
            TokenKind::Less => OperatorKind::LessThan,
            TokenKind::LessEqual => OperatorKind::LessThanOrEqualsTo,
            TokenKind::Greater => OperatorKind::GreaterThan,
            TokenKind::GreaterEqual => OperatorKind::GreaterThanOrEqualsTo,
            TokenKind::KwAnd => OperatorKind::LogicalAnd,
            TokenKind::KwOr => OperatorKind::LogicalOr,
            TokenKind::Dot => OperatorKind::MemberAccess,
            TokenKind::DoubleColon => OperatorKind::ScopeResolution,
            _ => OperatorKind::Invalid,
        };

        Operator {
            kind,
            raw_token: Some(tok),
            span: Some(SourceSpan { start, end, line }),
        }
    }
    // endregion Fragments
}
