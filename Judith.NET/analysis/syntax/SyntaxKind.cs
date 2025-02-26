using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

[JsonConverter(typeof(StringEnumConverter))]
public enum SyntaxKind
{
    CompilerUnit,

    // Top level directives
    ImportDirective,
    ModuleDirective,
    EnumerateDirective,

    // Definitions
    FunctionDefinition,
    UserTypeDefinition,
    AliasTypeDefinition,
    UnionTypeDefinition,
    SetTypeDefinition,
    StructTypeDefinition,
    InterfaceTypeDefinition,
    ClassTypeDefinition,

    // Implementations
    Implementation,
    ExtensionFunctionImplementation,
    ConstructorImplementation,
    InterfaceImplementation,

    // Statements
    LocalDeclarationStatement,
    BlockStatement,
    ArrowStatement,
    ReturnStatement,
    YieldStatement,
    BreakStatement,
    ContinueStatement,

    WhenStatement,

    ExpressionStatement,

    SingleFieldDeclarationExpression,
    MultipleFieldDeclarationExpression,

    // Control structure expressions
    IfExpression,
    MatchExpression,
    LoopExpression,
    WhileExpression,
    ForeachExpression,
    JumpTableExpression,

    // Other expressions
    AssignmentExpression,
    LogicalBinaryExpression,
    BinaryExpression,
    LeftUnaryExpression,

    // Primary expressions
    GroupExpression,
    AccessExpression,
    ObjectInitializationExpression,
    CallExpression,
    IdentifierExpression,
    LiteralExpression,

    Identifier,
    Literal,
    Operator,

    EqualsValueClause,
    MatchCase,
    Parameter,
    ParameterList,
    ArgumentList,
    Argument,
    TypeAnnotation,
    LocalDeclaratorList,
    LocalDeclarator,

    ObjectInitializer,
    FieldInitialization,
    MemberField,

    P_PrintStatement,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum LocalKind
{
    Constant, // const
    Variable, // var
}

[JsonConverter(typeof(StringEnumConverter))]
public enum LocalDeclaratorKind
{
    Regular, // const a, b = c
    ArrayPattern, // const [a, b] = ...c
    ObjectPattern, // const { a, b } = ...c
}

[JsonConverter(typeof(StringEnumConverter))]
public enum OperatorKind
{
    Add, // +
    Subtract, // -
    Multiply, // *
    Divide, // /
    BitwiseNot, // ~
    Assignment, // =
    Equals, // ==
    NotEquals, // !=
    Like, // ~=
    ReferenceEquals, // ===
    ReferenceNotEquals, // !==
    LessThan, // <
    LessThanOrEqualTo, // <=
    GreaterThan, // >
    GreaterThanOrEqualTo, // >=
    LogicalAnd, // and
    LogicalOr, // or
    MemberAccess, // .
    ScopeResolution, // ::
    //UserDefined, // % followed by identifier.
}

public enum AccessKind
{
    Scope,
    Member,
}