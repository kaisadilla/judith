using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

[JsonConverter(typeof(StringEnumConverter))]
public enum SyntaxKind {
    LocalDeclarationStatement,
    IfStatement,
    MatchStatement,
    LoopStatement,
    WhileStatement,
    ForeachStatement,
    ExpressionStatement,
    BlockStatement,
    ArrowStatement,

    EqualsValueClause,

    GroupExpression,

    SingleFieldDeclarationExpression,
    MultipleFieldDeclarationExpression,
    AssignmentExpression,
    BinaryExpression,
    LeftUnaryExpression,
    IdentifierExpression,
    LiteralExpression,

    Identifier,
    Literal,
    Operator,

    FieldDeclarator,
    MatchCase,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum FieldKind {
    Constant, // const
    Variable, // var
}

[JsonConverter(typeof(StringEnumConverter))]
public enum LiteralKind {
    Unknown,

    // Basic
    String,
    Number, // by default, float64
    Character,

    // Advanced (common name - suffix)
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
    Float64, // double - f64
    NativeInt, // nint - in
    NativeUnsignedInt, // nuint - un
    NativeFloat, // nfloat - fn
    Decimal, // decimal - d
}
