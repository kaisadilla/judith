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
    InvalidTopLevelStatement,
    InvalidIntegerLiteral,
    InvalidFloatLiteral,

    // 3xxx - Analyzer errors
    // SymbolResolver
    NameDoesNotExist = 3_000,
    // Binder
    NumberSuffixCannotBeUsedForDecimal,
    FloatLiteralOverflow,
    IntegerLiteralOverflow,
    UnsignedIntegerLiteralOverflow,
    IntegerLiteralIsTooLarge,
    UndefinedBinaryOperation,
    // TypeResolver
    TypeDoesntExist,
}
