use crate::judith::lexical::token::{Token, TokenKind};
use crate::judith::syntax::nodes::*;
use crate::SourceSpan;

pub struct SyntaxFactory;

impl SyntaxFactory {
    pub fn binary_expr(left: Expr, op: Operator, right: Expr) -> BinaryExpr {
        let start = left.span().unwrap().start;
        let end = right.span().unwrap().end;
        let line = left.span().unwrap().line;

        BinaryExpr {
            left,
            operator: op,
            right,
            span: Some(SourceSpan { start, end, line })
        }
    }

    pub fn left_unary_expr(op: Operator, expr: Expr) -> LeftUnaryExpr {
        let start = op.span.unwrap().start;
        let end = expr.span().unwrap().end;
        let line = op.span.unwrap().line;

        LeftUnaryExpr {
            operator: op,
            expr,
            span: Some(SourceSpan { start, end, line }),
        }
    }

    // region Expressions
    pub fn group_expr(left_paren: Token, expr: Expr, right_paren: Token) -> GroupExpr {
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

    pub fn object_init_expr(provider: Option<Expr>, initializer: ObjectInitializer) -> ObjectInitExpr {
        let start: i64;
        let end = initializer.span.unwrap().end;
        let line: i64;

        if let Some(provider) = &provider {
            start = provider.span().unwrap().start;
            line = provider.span().unwrap().line;
        }
        else {
            start = initializer.span.unwrap().start;
            line = initializer.span.unwrap().line;
        };

        ObjectInitExpr {
            provider,
            initializer,
            span: Some(SourceSpan { start, end, line }),
        }
    }

    pub fn access_expr(receiver: Option<Expr>, op: Operator, member: SimpleIdentifier) -> AccessExpr {
        let start: i64;
        let end = member.span.unwrap().end;
        let line: i64;

        if let Some(expr) = &receiver {
            start = expr.span().unwrap().start;
            line = expr.span().unwrap().line;
        }
        else {
            start = op.span.unwrap().start;
            line = op.span.unwrap().line;
        }

        AccessExpr {
            receiver,
            operator: op,
            member,
            span: Some(SourceSpan { start, end, line }),
        }
    }

    pub fn call_expr(callee: Expr, arguments: ArgumentList) -> CallExpr {
        let start = callee.span().unwrap().start;
        let end = arguments.span.unwrap().end;
        let line = callee.span().unwrap().line;

        CallExpr {
            callee,
            arguments,
            span: Some(SourceSpan { start, end, line }),
        }
    }

    pub fn identifier_expr(id: Identifier) -> IdentifierExpr {
        let span = id.span().clone();

        IdentifierExpr {
            identifier: id,
            span,
        }
    }

    pub fn literal_expr(literal: Literal) -> LiteralExpr {
        let span = literal.span.clone();

        LiteralExpr {
            literal,
            span,
        }
    }

    // endregion Expressions

    // region Fragments
    pub fn simple_identifier(tok: Token) -> SimpleIdentifier {
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

    pub fn qualified_identifier(
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

    pub fn literal(tok: Token) -> Literal {
        let start = tok.base().start;
        let end = tok.base().end;
        let line = tok.base().line;

        Literal {
            source: tok.base().lexeme.clone(),
            raw_token: Some(tok),
            span: Some(SourceSpan { start, end, line }),
        }
    }

    pub fn equals_value_clause(
        equals_token: Token, values: Vec<Expr>, comma_tokens: Vec<Token>
    ) -> EqualsValueClause {
        if values.len() == 0 {
            panic!("EqualsValueClause must have at least one value.");
        }

        let start = equals_token.base().start;
        let end = values.last().unwrap().span().unwrap().end;
        let line = equals_token.base().line;

        EqualsValueClause {
            values,
            span: Some(SourceSpan { start, end, line }),
            equals_token: Some(equals_token),
            comma_tokens: Some(comma_tokens),
        }
    }

    pub fn operator(tok: Token) -> Operator {
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
            TokenKind::TildeTilde => OperatorKind::Like,
            TokenKind::BangTilde => OperatorKind::NotLike,
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

    pub fn argument_list(
        left_paren: Token, args: Vec<Argument>, right_paren: Token, comma_tokens: Vec<Token>
    ) -> ArgumentList {
        let start = left_paren.base().start;
        let end = right_paren.base().end;
        let line = left_paren.base().line;

        ArgumentList {
            arguments: args,
            span: Some(SourceSpan { start, end, line }),
            left_paren_token: Some(left_paren),
            right_paren_token: Some(right_paren),
            comma_tokens: Some(comma_tokens),
        }
    }

    pub fn argument(expr: Expr) -> Argument {
        let span = expr.span().clone();
        Argument {
            expr,
            span,
        }
    }

    pub fn field_init(field_name: SimpleIdentifier, initializer: EqualsValueClause) -> FieldInit {
        let start = field_name.span.unwrap().start;
        let end = initializer.span.unwrap().end;
        let line = field_name.span.unwrap().line;

        FieldInit {
            field_name,
            initializer,
            span: Some(SourceSpan { start, end, line }),
        }
    }

    pub fn object_initializer(
        left_bracket: Token, field_inits: Vec<FieldInit>, right_bracket: Token, comma_tokens: Vec<Token>
    ) -> ObjectInitializer {
        let start = left_bracket.base().start;
        let end = right_bracket.base().end;
        let line = left_bracket.base().line;

        ObjectInitializer {
            field_inits,
            span: Some(SourceSpan { start, end, line }),
            left_bracket_token: Some(left_bracket),
            right_bracket_token: Some(right_bracket),
            comma_tokens: Some(comma_tokens),
        }
    }
    // endregion Fragments

    pub fn error_node() -> ErrorNode {
        ErrorNode {
            span: Some(SourceSpan { start: -1, end: -1, line: -1 }), // TODO
        }
    }
}
