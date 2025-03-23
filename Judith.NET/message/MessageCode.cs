using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Judith.NET.message;

[JsonConverter(typeof(StringEnumConverter))]
public enum MessageCode {
    // 1xxx - Syntax-related errors
    UnexpectedCharacter = 1_000,
    UnterminatedString,

    // 2xxx - Parsing errors
    UnexpectedToken = 2_000,
    IdentifierExpected,
    TypeAnnotationExpected,
    TypeExpected,
    LeftParenExpected,
    RightParenExpected,
    RightCurlyBracketExpected,
    RightSquareBracketExpected,
    ExpressionExpected,
    StatementExpected,
    BlockOpeningKeywordExpected,
    BodyExpected,
    ArrowExpected,
    ElsifBodyExpected,
    InExpected,
    DoExpected,
    EndExpected,
    ParameterExpected,
    ArgumentExpected,
    HidableItemExpected,
    LocalDeclaratorExpected,
    LocalDeclaratorListExpected,
    AssignmentExpressionExpected,
    InvalidTopLevelStatement,
    InvalidIntegerLiteral,
    InvalidFloatLiteral,
    ParameterTypeMustBeSpecified,
    FieldMustBeInitialized,

    // 3xxx - Analyzer errors
    // Agnostic analyzer
    InvalidExpressionForStatement = 3_000,
    InitializersMustMatchDeclarators,
    UninitializedDeclaratorsMustHaveType,

    // SymbolTableBuilder
    DefinitionAlreadyExist,

    // SymbolResolver
    NameDoesNotExist,
    NameIsAmbiguous,

    // Binder
    NumberSuffixCannotBeUsedForDecimal,
    FloatLiteralOverflow,
    IntegerLiteralOverflow,
    UnsignedIntegerLiteralOverflow,
    IntegerLiteralIsTooLarge,
    UndefinedBinaryOperation,

    // TypeResolver
    TypeDoesntExist,
    TypeExpectedTR,
    InvalidTypeForObjectInitialization,
    MemberAccessOnlyOnInstances,
    ScopeAccessNotOnInstances,
    FieldDoesNotExist,
    UnexpectedReturn,
    UnexpectedYield,
    UnexpectedYieldReturn,

    // BlockTypeResolver
    InconsistentReturnBehavior,

    // ImplicitNodeAnalyzer
    ReturnNotAllowed,
    YieldNotAllowed,
    NotAllPathsYieldValue,

    // TypeAnalyzer
    CannotAssignType,
}
