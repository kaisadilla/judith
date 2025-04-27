use serde::*;
use crate::judith::lexical::token::Token;
use crate::SourceSpan;

extern crate serde;

pub struct CompilerUnit {
    pub file_name: String,
    pub top_level_items: Vec<SyntaxNode>,
    pub impl_func: Option<FuncDef>,
}

// region Nodes
#[derive(Debug, Serialize)]
#[serde(tag = "node_kind")]
pub enum SyntaxNode {
    Item(Item),
    Stmt(Stmt),
    Expr(Expr),
    Error(ErrorNode),
}

impl SyntaxNode {
    pub fn span (&self) -> &Option<SourceSpan> {
        match self {
            SyntaxNode::Item(item) => item.span(),
            SyntaxNode::Stmt(expr) => expr.span(),
            SyntaxNode::Expr(expr) => expr.span(),
            SyntaxNode::Error(err) => &err.span,
        }
    }
}

#[derive(Debug, Serialize)]
#[serde(tag = "item_kind")]
// region Items
pub enum Item {
    FuncDef(FuncDef),
}

impl Item {
    pub fn span (&self) -> &Option<SourceSpan> {
        match self {
            Item::FuncDef(def) => &def.span,
        }
    }
}

#[derive(Debug, Serialize)]
pub struct FuncDef {
    pub is_implicit: bool,
    pub name: SimpleIdentifier,
    pub params: ParameterList,
    pub return_type: Option<TypeNode>,
    pub body: Body,
    pub span: Option<SourceSpan>,
    pub func_token: Option<Token>,
    pub return_type_arrow_token: Option<Token>,
}
// endregion

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
    Expr(ExprStmt),
    LocalDecl(LocalDeclStmt),
    Error(ErrorNode),
}

impl Stmt {
    pub fn span (&self) -> &Option<SourceSpan> {
        match self {
            Stmt::Expr(stmt) => &stmt.span,
            Stmt::LocalDecl(stmt) => &stmt.span,
            Stmt::Error(stmt) => &stmt.span,
        }
    }
}

#[derive(Debug, Serialize)]
pub struct ExprStmt {
    pub expr: Expr,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct LocalDeclStmt {
    pub decl: PartialLocalDecl,
    pub initializer: Option<EqualsValueClause>,
    pub span: Option<SourceSpan>,
    pub let_token: Option<Token>,
}

#[derive(Debug, Serialize)]
#[serde(tag = "decl_type")]
pub enum PartialLocalDecl {
    Regular(RegularLocalDecl),
    Destructured(DestructuredLocalDecl),
}

impl PartialLocalDecl {
    pub fn span(&self) -> &Option<SourceSpan> {
        match &self {
            PartialLocalDecl::Regular(decl) => &decl.span,
            PartialLocalDecl::Destructured(decl) => &decl.span,
        }
    }
}

#[derive(Debug, Serialize)]
pub struct RegularLocalDecl {
    pub declarator: LocalDeclarator,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct DestructuredLocalDecl {
    pub declarators: Vec<LocalDeclarator>,
    pub destructuring_kind: DestructuringKind,
    pub span: Option<SourceSpan>,
    pub opening_token: Option<Token>,
    pub closing_token: Option<Token>,
}

// endregion Statements

// region Expressions
#[derive(Debug, Serialize)]
#[serde(tag = "expr_kind")]
pub enum Expr {
    If(Box<IfExpr>),
    Loop(Box<LoopExpr>),
    While(Box<WhileExpr>),
    Assignment(Box<AssignmentExpr>),
    Binary(Box<BinaryExpr>),
    LeftUnary(Box<LeftUnaryExpr>),
    Group(Box<GroupExpr>),
    ObjectInit(Box<ObjectInitExpr>),
    Access(Box<AccessExpr>),
    Call(Box<CallExpr>),
    Identifier(Box<IdentifierExpr>),
    Literal(Box<LiteralExpr>),
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
pub struct ParameterList {
    pub params: Vec<Parameter>,
    pub span: Option<SourceSpan>,
    pub left_paren_token: Option<Token>,
    pub right_paren_token: Option<Token>,
    pub comma_tokens: Option<Vec<Token>>,
}

#[derive(Debug, Serialize)]
pub struct Parameter {
    pub declarator: RegularLocalDecl,
    pub default_val: Option<EqualsValueClause>,
    pub span: Option<SourceSpan>,
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
pub struct LocalDeclarator {
    pub ownership_kind: OwnershipKind,
    pub name: SimpleIdentifier,
    pub type_annotation: Option<TypeAnnotation>,
    pub span: Option<SourceSpan>,
    pub ownership_token: Option<Token>,
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
pub struct TypeNode {
    pub is_nullable: bool,
    pub ownership_kind: OwnershipKind,
    pub ty: PartialType,
    pub nullable_token: Option<Token>,
    pub ownership_token: Option<Token>,
}

impl TypeNode {
    pub fn span(&self) -> &Option<SourceSpan> {
        match &self.ty {
            PartialType::Identifier(ty) => &ty.span,
            PartialType::Group(ty) => &ty.span,
            PartialType::Function(ty) => &ty.span,
            PartialType::TupleArray(ty) => &ty.span,
            PartialType::RawArray(ty) => &ty.span,
            PartialType::Literal(ty) => &ty.span,
            PartialType::Sum(ty) => &ty.span,
            PartialType::Product(ty) => &ty.span,
            PartialType::Error(err) => &err.span,
        }
    }
}

#[derive(Debug, Serialize)]
#[serde(tag = "type_kind")]
pub enum PartialType {
    Identifier(IdentifierType),
    Group(Box<GroupType>),
    Function(Box<FunctionType>),
    // TODO ObjectType
    TupleArray(Box<TupleArrayType>),
    RawArray(Box<RawArrayType>),
    Literal(LiteralType),
    Sum(Box<SumType>),
    Product(Box<ProductType>),
    Error(ErrorNode),
}

#[derive(Debug, Serialize)]
pub struct IdentifierType {
    pub name: Identifier,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct GroupType {
    pub ty: TypeNode,
    pub span: Option<SourceSpan>,
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
    pub span: Option<SourceSpan>,
    pub ss_token: Option<Token>,
    pub exception_mark_token: Option<Token>,
    pub left_paren_token: Option<Token>,
    pub right_paren_token: Option<Token>,
    pub param_comma_tokens: Option<Vec<Token>>,
    pub return_annotation_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct TupleArrayType {
    pub member_types: Vec<TypeNode>,
    pub span: Option<SourceSpan>,
    pub left_square_bracket_token: Option<Token>,
    pub right_square_bracket_token: Option<Token>,
    pub comma_tokens: Option<Vec<Token>>,
}

#[derive(Debug, Serialize)]
pub struct RawArrayType {
    pub member_type: TypeNode,
    pub length: Expr,
    pub span: Option<SourceSpan>,
    pub left_square_bracket_token: Option<Token>,
    pub right_square_bracket_token: Option<Token>,
}

#[derive(Debug, Serialize)]
pub struct LiteralType {
    pub literal: Literal,
    pub span: Option<SourceSpan>,
}

#[derive(Debug, Serialize)]
pub struct SumType {
    pub member_types: Vec<TypeNode>,
    pub span: Option<SourceSpan>,
    pub or_tokens: Option<Vec<Token>>,
}

#[derive(Debug, Serialize)]
pub struct ProductType {
    pub member_types: Vec<TypeNode>,
    pub span: Option<SourceSpan>,
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
    Final, // final
    Mutable, // mut
    Shared, // sh
    Reference, // ref
    In, // in
}

#[derive(Debug, Clone, PartialEq, Serialize)]
pub enum DestructuringKind {
    ArrayPattern, // let [a, b]
    ObjectPattern, // let { a, b }
}
// endregion

impl CompilerUnit {
    pub fn from_node_collection(filename: &String, ast: Vec<SyntaxNode>) -> CompilerUnit {
        let mut top_level_items: Vec<SyntaxNode> = Vec::new();
        let mut implicit_func_nodes: Vec<SyntaxNode> = Vec::new();

        for node in ast {
            match node {
                SyntaxNode::Item(item) => top_level_items.push(SyntaxNode::Item(item)),
                SyntaxNode::Stmt(stmt) => implicit_func_nodes.push(SyntaxNode::Stmt(stmt)),
                SyntaxNode::Expr(expr) => panic!("Invalid top level node."),
                SyntaxNode::Error(err) => top_level_items.push(SyntaxNode::Error(err)),
            }
        }

        let implicit_func = if implicit_func_nodes.len() > 0 {
            let param_list = ParameterList {
                params: Vec::new(),
                span: None,
                left_paren_token: None,
                right_paren_token: None,
                comma_tokens: None,
            };

            let body = BlockBody {
                nodes: implicit_func_nodes,
                span: None,
                opening_token: None,
                closing_token: None,
            };

            Some(FuncDef {
                is_implicit: true,
                name: SimpleIdentifier {
                    is_meta_name: true,
                    name: String::from("!implicit_func"),
                    is_escaped: false,
                    raw_token: None,
                    span: None,
                },
                params: param_list,
                return_type: None,
                body: Body::Block(body),

                span: None,
                func_token: None,
                return_type_arrow_token: None,
            })
        }
        else {
            None
        };

        CompilerUnit {
            file_name: filename.clone(),
            top_level_items,
            impl_func: implicit_func,
        }
    }
}