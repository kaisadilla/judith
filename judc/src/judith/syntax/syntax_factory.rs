use crate::judith::lexical::token::{Token, TokenKind};
use crate::judith::syntax::nodes::*;
use crate::SourceSpan;

pub struct SyntaxFactory;

impl SyntaxFactory {
    // region Bodies
    pub fn block_body(
        opening_token: Option<Token>, nodes: Vec<SyntaxNode>, closing_token: Option<Token>
    ) -> BlockBody {
        let start: i64;
        let end: i64;
        let line: i64;

        if let Some(tok) = &opening_token {
            start = tok.base().start;
            line = tok.base().line;
        } else if let Some(node) = nodes.last() {
            start = node.span().unwrap().start;
            line = node.span().unwrap().end;
        } else if let Some(tok) = &closing_token {
            start = tok.base().start;
            line = tok.base().line;
        }
        else {
            panic!("Block body must contain, at least, one token.");
        };

        if let Some(tok) = &closing_token {
            end = tok.base().end;
        }
        else if let Some(node) = nodes.last() {
            end = node.span().unwrap().end;
        }
        else if let Some(tok) = &opening_token {
            end = tok.base().end;
        }
        else {
            panic!("Block body must contain, at least, one token.");
        }

        BlockBody {
            opening_token,
            nodes,
            closing_token,
            span: Some(SourceSpan { start, end, line }),
        }
    }

    pub fn arrow_body(arrow_tok: Token, expr: Expr) -> ArrowBody {
        let start = arrow_tok.base().start;
        let end = expr.span().unwrap().end;
        let line = arrow_tok.base().line;

        ArrowBody {
            arrow_token: Some(arrow_tok),
            expr,
            span: Some(SourceSpan { start, end, line }),
        }
    }

    pub fn expr_body(expr: Expr) -> ExprBody {
        let start = expr.span().unwrap().start;
        let end = expr.span().unwrap().end;
        let line = expr.span().unwrap().line;

        ExprBody {
            expr,
            span: Some(SourceSpan { start, end, line }),
        }
    }
    // endregion

    // region Statements
    pub fn expr_stmt(expr: Expr) -> ExprStmt {
        let span = expr.span().unwrap().clone();

        ExprStmt {
            expr,
            span: Some(span),
        }
    }

    pub fn local_decl_stmt(
        let_tok: Token,
        decl: PartialLocalDecl,
        init: Option<EqualsValueClause>
    ) -> LocalDeclStmt {
        let start = let_tok.base().start;
        let end: i64;
        let line = let_tok.base().line;

        if let Some(init) = &init {
            end = init.span.unwrap().end;
        }
        else {
            end = decl.span().unwrap().end;
        }

        LocalDeclStmt {
            decl,
            initializer: init,
            span: Some(SourceSpan { start, end, line }),
            let_token: Some(let_tok),
        }
    }

    pub fn regular_local_decl(declarator: LocalDeclarator) -> RegularLocalDecl {
        let span = declarator.span.unwrap().clone();

        RegularLocalDecl {
            declarator,
            span: Some(span),
        }
    }

    pub fn destructured_local_decl(
        opening_tok: Token, declarators: Vec<LocalDeclarator>, closing_tok: Token
    ) -> DestructuredLocalDecl {
        panic_when_invalid_pair(&opening_tok, &closing_tok);

        let start = opening_tok.base().start;
        let end = closing_tok.base().end;
        let line = opening_tok.base().line;

        let destructuring_kind = match &opening_tok.kind() {
            TokenKind::LeftSquareBracket => DestructuringKind::ArrayPattern,
            TokenKind::LeftCurlyBracket => DestructuringKind::ObjectPattern,
            _ => panic!("Invalid destructuring token."),
        };

        DestructuredLocalDecl {
            declarators,
            destructuring_kind,
            span: Some(SourceSpan { start, end, line }),
            opening_token: Some(opening_tok),
            closing_token: Some(closing_tok),
        }
    }
    // endregion Statements

    // region Expressions
    pub fn if_expr(if_tok: Token, test: Expr, body: Body) -> IfExpr {
        let start = if_tok.base().start;
        let end = body.span().unwrap().end;
        let line = if_tok.base().line;

        IfExpr {
            test,
            consequent: body,
            alternate: None,
            span: Some(SourceSpan { start, end, line }),
            if_token: Some(if_tok),
            else_token: None,
        }
    }

    pub fn if_else_expr(
        if_tok: Token, test: Expr, consequent: Body, else_tok: Option<Token>, alternate: Body
    ) -> IfExpr {
        let start = if_tok.base().start;
        let end = alternate.span().unwrap().end;
        let line = if_tok.base().line;

        IfExpr {
            test,
            consequent,
            alternate: Some(alternate),
            span: Some(SourceSpan { start, end, line }),
            if_token: Some(if_tok),
            else_token: else_tok,
        }
    }

    pub fn loop_expr(loop_tok: Token, body: Body) -> LoopExpr {
        let start = loop_tok.base().start;
        let end = body.span().unwrap().end;
        let line = loop_tok.base().line;

        LoopExpr {
            body,
            span: Some(SourceSpan { start, end, line }),
            loop_token: Some(loop_tok),
        }
    }

    pub fn while_expr(while_tok: Token, test: Expr, body: Body) -> WhileExpr {
        let start = while_tok.base().start;
        let end = body.span().unwrap().end;
        let line = while_tok.base().line;

        WhileExpr {
            test,
            body,
            span: Some(SourceSpan { start, end, line }),
            while_token: Some(while_tok),
        }
    }

    // region Cascading expressions
    pub fn assignment_expr(left: Expr, op: Operator, right: Expr) -> AssignmentExpr {
        let start = left.span().unwrap().start;
        let end = right.span().unwrap().end;
        let line = left.span().unwrap().line;

        AssignmentExpr {
            left,
            operator: op,
            right,
            span: Some(SourceSpan { start, end, line }),
        }
    }

    pub fn binary_expr(left: Expr, op: Operator, right: Expr) -> BinaryExpr {
        let start = left.span().unwrap().start;
        let end = right.span().unwrap().end;
        let line = left.span().unwrap().line;

        BinaryExpr {
            left,
            operator: op,
            right,
            span: Some(SourceSpan { start, end, line }),
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
    // endregion Cascading expressions

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

    pub fn type_annotation(colon: Token, ty: TypeNode) -> TypeAnnotation {
        let start = colon.base().start;
        let end = ty.span().unwrap().end;
        let line = colon.base().line;

        TypeAnnotation {
            ty,
            span: Some(SourceSpan { start, end, line }),
            colon_token: Some(colon),
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

    pub fn local_declarator(
        ownership_tok: Option<Token>, name: SimpleIdentifier, ty: Option<TypeAnnotation>
    ) -> LocalDeclarator {
        let start = name.span.unwrap().start;
        let end: i64;
        let line = name.span.unwrap().line;

        if let Some(ty) = &ty {
            end = ty.span.unwrap().end;
        }
        else {
            end = name.span.unwrap().end;
        };

        let ownership_kind = get_ownership(&ownership_tok);

        LocalDeclarator {
            ownership_kind,
            name,
            type_annotation: ty,
            span: Some(SourceSpan { start, end, line }),
            ownership_token: ownership_tok,
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

    //region Type nodes
    pub fn type_node(
        ownership_tok: Option<Token>, ty: PartialType, nullable_tok: Option<Token>
    ) -> TypeNode {
        let is_nullable = match &nullable_tok {
            Some(tok) if tok.kind() == TokenKind::QuestionMark => true,
            None => false,
            _ => panic!("'nullable_tok' can only be None or '?'."),
        };

        let ownership_kind = get_ownership(&ownership_tok);

        TypeNode {
            is_nullable,
            ownership_kind,
            ty,
            nullable_token: nullable_tok,
            ownership_token: ownership_tok,
        }
    }

    pub fn identifier_type(id: Identifier) -> IdentifierType {
        let span = id.span().unwrap().clone();

        IdentifierType {
            name: id,
            span: Some(span),
        }
    }

    pub fn group_type(left_paren: Token, ty: TypeNode, right_paren: Token) -> GroupType {
        let start = left_paren.base().start;
        let end = right_paren.base().end;
        let line = left_paren.base().line;

        GroupType {
            ty,
            span: Some(SourceSpan { start, end, line }),
            left_paren_token: Some(left_paren),
            right_paren_token: Some(right_paren),
        }
    }

    pub fn function_type(
        ss: Option<Token>,
        except: Option<Token>,
        left_paren: Token,
        param_types: Vec<TypeNode>,
        right_paren: Token,
        param_comma_tokens: Vec<Token>,
        return_annotation: Token,
        return_type: TypeNode
    ) -> FunctionType {
        let start: i64;
        let end = return_type.span().unwrap().end;
        let line: i64;

        if let Some(tok) = &ss {
            start = tok.base().start;
            line = tok.base().line;
        }
        else if let Some(tok) = &except {
            start = tok.base().start;
            line = tok.base().line;
        }
        else {
            start = left_paren.base().start;
            line = left_paren.base().line;
        };

        let is_send: bool;
        let is_sync: bool;
        let has_exception: bool;

        match &ss {
            Some(tok) if tok.base().lexeme == "s" => { is_send = true; is_sync = false; },
            Some(tok) if tok.base().lexeme == "S" => { is_send = false; is_sync = true; },
            Some(tok) if tok.base().lexeme == "sS" => { is_send = true; is_sync = true; },
            None => { is_send = false; is_sync = false; },
            _ => panic!("ss token can only contain None, 's', 'sS' or 'S'."),
        }

        match &except {
            Some(tok) if tok.kind() == TokenKind::Bang => has_exception = true,
            None => has_exception = false,
            _ => panic!("except token can only contain None or '!'."),
        }

        FunctionType {
            param_types,
            return_type,
            is_send,
            is_sync,
            has_exception,
            span: Some(SourceSpan { start, end, line }),
            ss_token: ss,
            exception_mark_token: except,
            left_paren_token: Some(left_paren),
            right_paren_token: Some(right_paren),
            param_comma_tokens: Some(param_comma_tokens),
            return_annotation_token: Some(return_annotation),
        }
    }

    pub fn tuple_array_type(
        left_bracket: Token, member_types: Vec<TypeNode>, right_bracket: Token, comma_tokens: Vec<Token>
    ) -> TupleArrayType {
        let start = left_bracket.base().start;
        let end = right_bracket.base().end;
        let line = left_bracket.base().line;

        TupleArrayType {
            member_types,
            span: Some(SourceSpan { start, end, line }),
            left_square_bracket_token: Some(left_bracket),
            right_square_bracket_token: Some(right_bracket),
            comma_tokens: Some(comma_tokens),
        }
    }

    pub fn raw_array_type(
        member_type: TypeNode, left_bracket: Token, len: Expr, right_bracket: Token
    ) -> RawArrayType {
        let start = member_type.span().unwrap().start;
        let end = right_bracket.base().end;
        let line = member_type.span().unwrap().start;

        RawArrayType {
            member_type,
            length: len,
            span: Some(SourceSpan { start, end, line }),
            left_square_bracket_token: Some(left_bracket),
            right_square_bracket_token: Some(right_bracket),
        }
    }

    pub fn literal_type(lit: Literal) -> LiteralType {
        let span = lit.span.unwrap().clone();

        LiteralType {
            literal: lit,
            span: Some(span),
        }
    }

    pub fn sum_type(member_types: Vec<TypeNode>, or_tokens: Vec<Token>) -> SumType {
        if member_types.len() == 0 {
            panic!("A sum type must contain, at least, one type.");
        };

        let start = member_types.first().unwrap().span().unwrap().start;
        let end = member_types.last().unwrap().span().unwrap().end;
        let line = member_types.first().unwrap().span().unwrap().line;

        SumType {
            member_types,
            span: Some(SourceSpan { start, end, line }),
            or_tokens: Some(or_tokens),
        }
    }

    pub fn product_type(member_types: Vec<TypeNode>, and_tokens: Vec<Token>) -> ProductType {
        if member_types.len() == 0 {
            panic!("A product type must contain, at least, one type.");
        };

        let start = member_types.first().unwrap().span().unwrap().start;
        let end = member_types.last().unwrap().span().unwrap().end;
        let line = member_types.first().unwrap().span().unwrap().line;

        ProductType {
            member_types,
            span: Some(SourceSpan { start, end, line }),
            and_tokens: Some(and_tokens),
        }
    }
    //endregion Type nodes

    pub fn error_node() -> ErrorNode {
        ErrorNode {
            span: Some(SourceSpan { start: -1, end: -1, line: -1 }), // TODO
        }
    }
}

fn get_ownership(tok: &Option<Token>) -> OwnershipKind {
    match &tok {
        Some(tok) if tok.kind() == TokenKind::KwFinal => OwnershipKind::Final,
        Some(tok) if tok.kind() == TokenKind::KwMut => OwnershipKind::Mut,
        Some(tok) if tok.kind() == TokenKind::KwIn => OwnershipKind::In,
        Some(tok) if tok.kind() == TokenKind::KwShared => OwnershipKind::Shared,
        Some(tok) if tok.kind() == TokenKind::KwRef => OwnershipKind::Ref,
        None => OwnershipKind::None,
        _ => panic!("Ownership token can only be None, 'final', 'mut', 'in', 'shared' or 'ref'."),
    }
}

/// Checks the token kinds given and panics if they don't form a pair. Valid pairs:
/// * `(` and `)`.
/// * `[` and `]`.
/// * `{` and `}`.
/// * `<` and `>` only when they are `LeftAngleBracket` and `RightAngleBracket`.
fn panic_when_invalid_pair(left: &Token, right: &Token) {
    if left.kind() == TokenKind::LeftParen {
        if right.kind() == TokenKind::RightParen {
            return;
        }
    }
    if left.kind() == TokenKind::LeftSquareBracket {
        if right.kind() == TokenKind::RightSquareBracket {
            return;
        }
    }
    if left.kind() == TokenKind::LeftCurlyBracket {
        if right.kind() == TokenKind::RightCurlyBracket {
            return;
        }
    }
    if left.kind() == TokenKind::LeftAngleBracket {
        if right.kind() == TokenKind::RightAngleBracket {
            return;
        }
    }

    panic!("Invalid token pair.");
}