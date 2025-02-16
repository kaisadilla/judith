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

    GroupExpression,

    SingleFieldDeclarationExpression,
    MultipleFieldDeclarationExpression,

    // Control structure expressions
    IfExpression,
    MatchExpression,
    LoopExpression,
    WhileExpression,
    ForeachExpression,

    AssignmentExpression,
    BinaryExpression,
    LeftUnaryExpression,
    AccessExpression,
    IdentifierExpression,
    LiteralExpression,

    Identifier,
    Literal,
    Operator,

    EqualsValueClause,
    MatchCase,
    Parameter,
    ParameterList,
    TypeAnnotation,
    LocalDeclaratorList,
    LocalDeclarator,

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
public enum LiteralKind
{
    Unknown,

    // Basic
    String,
    Character,
    Keyword, // literals like 'true' or 'null'.

    // Numbers (common name - suffix)
    Int8, // sbyte - i8
    Int16, // short - i16
    Int32, // int - i32
    Int64, // long - i64
    UnsignedInt8, // byte - u8
    UnsignedInt16, // ushort - u16
    UnsignedInt32, // uint - u32
    UnsignedInt64, // ulong - u64
    Float16, // single - f16
    Float32, // float - f32
    Float64, // double - f64 (can appear without suffix)
    NativeInt, // nint - in
    NativeUnsignedInt, // nuint - un
    NativeFloat, // nfloat - fn
    Decimal, // decimal - d
}

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