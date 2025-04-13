use serde::*;
use crate::judith::lexical::token::Token;
use crate::SourceSpan;

extern crate serde;

// region Nodes
#[derive(Debug, Serialize)]
#[serde(tag = "node_kind")]
pub enum SyntaxNode {
    Stmt(Stmt),
    Expr(Expr),
    Error(ErrorNode),
}

impl SyntaxNode {
    pub fn span (&self) -> &Option<SourceSpan> {
        match self {
            SyntaxNode::Stmt(expr) => expr.span(),
            SyntaxNode::Expr(expr) => expr.span(),
            SyntaxNode::Error(err) => &err.span,
        }
    }
}

// region Bodies
#[derive(Debug, Serialize)]
#[serde(tag = "block_kind")]
pub enum Body {
    #[serde(rename = "Block")]
    Block(BlockBody),

    #[serde(rename = "Arrow")]
    Arrow(ArrowBody),

    #[serde(rename = "Expr")]
    Expr(ExprBody),
}

impl Body {
    pub fn span (&self) -> &Option<SourceSpan> {
        match self {
            Body::Block(b) => &b.span,
            Body::Arrow(b) => &b.span,
            Body::Expr(b) => &b.span,
        }
    }
}

#[derive(Debug, Serialize)]
pub struct BlockBody {
    pub nodes: Vec<SyntaxNode>,
    pub span: Option<SourceSpan>,
    pub opening_token: Option<Token>,
    pub closing_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct ArrowBody {
    pub expr: Expr,
    pub span: Option<SourceSpan>,
    pub arrow_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct ExprBody {
    pub expr: Expr,
    pub span: Option<SourceSpan>,
}
// endregion Bodies

// region Statements
#[derive(Debug, Serialize)]
#[serde(tag = "stmt_kind")]
pub enum Stmt {
    #[serde(rename = "Expr")]
    Expr(ExprStmt),

    #[serde(rename = "Error")]
    Error(ErrorNode),
}

impl Stmt {
    pub fn span (&self) -> &Option<SourceSpan> {
        match self {
            Stmt::Expr(stmt) => &stmt.span,
            Stmt::Error(stmt) => &stmt.span,
        }
    }
}

#[derive(Debug, Serialize)]
pub struct ExprStmt {
    pub expr: Expr,
    pub span: Option<SourceSpan>,
}
// endregion Statements

// region Expressions
#[derive(Debug, Serialize)]
#[serde(tag = "expr_kind")]
pub enum Expr {
    #[serde(rename = "If")]
    If(Box<IfExpr>),

    #[serde(rename = "Loop")]
    Loop(Box<LoopExpr>),

    #[serde(rename = "While")]
    While(Box<WhileExpr>),

    #[serde(rename = "Assignment")]
    Assignment(Box<AssignmentExpr>),

    #[serde(rename = "Binary")]
    Binary(Box<BinaryExpr>),

    #[serde(rename = "LeftUnary")]
    LeftUnary(Box<LeftUnaryExpr>),

    #[serde(rename = "Group")]
    Group(Box<GroupExpr>),

    #[serde(rename = "ObjectInit")]
    ObjectInit(Box<ObjectInitExpr>),

    #[serde(rename = "Access")]
    Access(Box<AccessExpr>),

    #[serde(rename = "Call")]
    Call(Box<CallExpr>),

    #[serde(rename = "Identifier")]
    Identifier(Box<IdentifierExpr>),

    #[serde(rename = "Literal")]
    Literal(Box<LiteralExpr>),

    #[serde(rename = "Error")]
    Error(ErrorNode),
}

impl Expr {
    pub fn span (&self) -> &Option<SourceSpan> {
        match self {
            Expr::If(expr) => &expr.span,
            Expr::Loop(expr) => &expr.span,
            Expr::While(expr) => &expr.span,
            Expr::Assignment(expr) => &expr.span,
            Expr::Binary(expr) => &expr.span,
            Expr::LeftUnary(expr) => &expr.span,
            Expr::Group(expr) => &expr.span,
            Expr::ObjectInit(expr) => &expr.span,
            Expr::Access(expr) => &expr.span,
            Expr::Call(expr) => &expr.span,
            Expr::Identifier(expr) => &expr.span,
            Expr::Literal(expr) => &expr.span,
            Expr::Error(expr) => &expr.span,
        }
    }
}

#[derive(Debug, Serialize)]
pub struct IfExpr {
    pub test: Expr,
    pub consequent: Body,
    pub alternate: Option<Body>,
    pub span: Option<SourceSpan>,
    pub if_token: Option<Token>,
    pub else_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct LoopExpr {
    pub body: Body,
    pub span: Option<SourceSpan>,
    pub loop_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct WhileExpr {
    pub test: Expr,
    pub body: Body,
    pub span: Option<SourceSpan>,
    pub while_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct AssignmentExpr {
    pub left: Expr,
    pub operator: Operator,
    pub right: Expr,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct BinaryExpr {
    pub left: Expr,
    pub operator: Operator,
    pub right: Expr,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct LeftUnaryExpr {
    pub operator: Operator,
    pub expr: Expr,
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
pub struct ObjectInitExpr {
    pub provider: Option<Expr>,
    pub initializer: ObjectInitializer,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct AccessExpr {
    pub receiver: Option<Expr>,
    pub operator: Operator,
    pub member: SimpleIdentifier,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct CallExpr {
    pub callee: Expr,
    pub arguments: ArgumentList,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct IdentifierExpr {
    pub identifier: Identifier,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct LiteralExpr {
    pub literal: Literal,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct ErrorNode {
    pub span: Option<SourceSpan>,
}
// endregion Expressions

// region Fragments
#[derive(Debug, Serialize)]
#[serde(tag = "identifier_kind")]
pub enum Identifier {
    #[serde(rename = "Simple")]
    Simple(SimpleIdentifier),

    #[serde(rename = "Qualified")]
    Qualified(Box<QualifiedIdentifier>),
}

impl Identifier {
    pub fn is_meta_name (&self) -> bool {
        match self {
            Identifier::Simple(id) => id.is_meta_name,
            Identifier::Qualified(id) => id.is_meta_name,
        }
    }

    pub fn span (&self) -> &Option<SourceSpan> {
        match self {
            Identifier::Simple(id) => &id.span,
            Identifier::Qualified(id) => &id.span,
        }
    }
}

#[derive(Debug, Serialize)]
pub struct SimpleIdentifier {
    pub is_meta_name: bool,
    pub name: String,
    pub is_escaped: bool,
    pub raw_token: Option<Token>,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct QualifiedIdentifier {
    pub is_meta_name: bool,
    pub qualifier: Identifier,
    pub operator: Operator,
    pub name: SimpleIdentifier,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct Literal {
    pub source: String,
    pub span: Option<SourceSpan>,
    pub raw_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct EqualsValueClause {
    pub values: Vec<Expr>,
    pub span: Option<SourceSpan>,
    pub equals_token: Option<Token>,
    pub comma_tokens: Option<Vec<Token>>,
}

#[derive(Debug, Serialize)]
pub struct TypeAnnotation {
    pub ty: TypeNode,
    pub span: Option<SourceSpan>,
    pub colon_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct Operator {
    pub kind: OperatorKind,
    pub span: Option<SourceSpan>,
    pub raw_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct ArgumentList {
    pub arguments: Vec<Argument>,
    pub span: Option<SourceSpan>,
    pub left_paren_token: Option<Token>,
    pub right_paren_token: Option<Token>,
    pub comma_tokens: Option<Vec<Token>>,
}

#[derive(Debug, Serialize)]
pub struct Argument {
    pub expr: Expr,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct FieldInit {
    pub field_name: SimpleIdentifier,
    pub initializer: EqualsValueClause,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct ObjectInitializer {
    pub field_inits: Vec<FieldInit>,
    pub span: Option<SourceSpan>,
    pub left_bracket_token: Option<Token>,
    pub right_bracket_token: Option<Token>,
    pub comma_tokens: Option<Vec<Token>>,
}
// endregion Fragments

// region Type nodes
#[derive(Debug, Serialize)]
pub enum TypeNode {
    Identifier(TypeNodeInfo, IdentifierType),
    Group(TypeNodeInfo, Box<GroupType>),
    Function(TypeNodeInfo, Box<FunctionType>),
    // TODO ObjectType
    TupleArray(TypeNodeInfo, Box<TupleArrayType>),
    RawArray(TypeNodeInfo, Box<RawArrayType>),
    Literal(TypeNodeInfo, LiteralType),
    Sum(TypeNodeInfo, Box<SumType>),
    Product(TypeNodeInfo, Box<ProductType>),
}

#[derive(Debug, Serialize)]
pub struct TypeNodeInfo {
    pub is_nullable: bool,
    pub ownership_kind: OwnershipKind,
    pub span: Option<SourceSpan>,
    pub nullable_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct IdentifierType {
    pub name: Identifier,
}

#[derive(Debug, Serialize)]
pub struct GroupType {
    pub ty: TypeNode,
    pub left_paren_token: Option<Token>,
    pub right_paren_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct FunctionType {
    pub param_types: Vec<TypeNode>,
    pub return_type: TypeNode,
    pub is_send: bool,
    pub is_sync: bool,
    pub has_exception: bool,
    pub ss_token: Option<Token>,
    pub exception_mark_token: Option<Token>,
    pub left_paren_token: Option<Token>,
    pub right_paren_token: Option<Token>,
    pub return_annotation_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct TupleArrayType {
    pub member_types: Vec<TypeNode>,
    pub left_square_bracket_token: Option<Token>,
    pub right_square_bracket_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct RawArrayType {
    pub member_type: Vec<TypeNode>,
    pub length: Expr,
    pub left_square_bracket_token: Option<Token>,
    pub right_square_bracket_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct LiteralType {
    pub literal: Literal,
}

#[derive(Debug, Serialize)]
pub struct SumType {
    pub member_types: Vec<TypeNode>,
    pub or_tokens: Option<Vec<Token>>,
}

#[derive(Debug, Serialize)]
pub struct ProductType {
    pub member_types: Vec<TypeNode>,
    pub and_tokens: Option<Vec<Token>>,
}
// endregion

// endregion Nodes

// region Enums
#[derive(Debug, Clone, PartialEq, Serialize)]
pub enum OperatorKind {
    Invalid,
    Add, // +
    Subtract, // -
    Multiply, // *
    Divide, // /
    BitwiseNot, // ~
    Assignment, // =
    Equals, // ==
    NotEquals, // !=
    Like, // ~~
    NotLike, // !~
    ReferenceEquals, // ===
    ReferenceNotEquals, // !==
    LessThan, // <
    LessThanOrEqualsTo, // !==
    GreaterThan, // >
    GreaterThanOrEqualsTo, // >=
    LogicalAnd, // and
    LogicalOr, // or
    MemberAccess, // .
    ScopeResolution, // ::
}

#[derive(Debug, Clone, PartialEq, Serialize)]
pub enum OwnershipKind {
    /// This node doesn't define any ownership kind.
    None,
    Final,
    Mut,
    Shared,
    Ref,
    In,
}
// endregion