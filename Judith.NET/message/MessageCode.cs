using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.message;

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
    HidableItemExpected,
    LocalDeclaratorExpected,
    LocalDeclaratorListExpected,
    InvalidTopLevelStatement,
    InvalidIntegerLiteral,
    InvalidFloatLiteral,
}
