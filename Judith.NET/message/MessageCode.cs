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
    ExpressionExpected,
    FieldDeclarationExpected,
    BlockOpeningKeywordExpected,
    ArrowExpected,
    InExpected,
    DoExpected,
    EndExpected,
    ParameterExpected,
    InvalidTopLevelStatement,
}
