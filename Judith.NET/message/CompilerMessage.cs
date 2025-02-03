using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.message;

public enum MessageKind {
    Information,
    Warning,
    Error,
}

public enum MessageOrigin {
    Lexer,
    Parser,
    Compiler,
}

public class CompilerMessage {
    public MessageKind Kind { get; init; }
    public MessageOrigin Origin { get; init; }
    public int Code { get; init; }
    public string Message { get; init; }

    private CompilerMessage (
        MessageKind kind, MessageOrigin origin, int code, string message
    ) {
        Kind = kind;
        Origin = origin;
        Code = code;
        Message = message;
    }

    public override string ToString () {
        return $"[{Origin}/{Kind}] {Code,4} - {Message}";
    }

    public static class Lexer {
        public static CompilerMessage UnexpectedCharacter (char unexpectedChar) {
            return new(
                MessageKind.Error,
                MessageOrigin.Lexer,
                (int)MessageCode.UnexpectedCharacter,
                $"Unexpected character: '{unexpectedChar}'."
            );
        }
    }

    public static class Parser {
        public static CompilerMessage IdentifierExpected () {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.IdentifierExpected,
                $"Identifier expected."
            );
        }

        public static CompilerMessage TypeExpected () {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.TypeExpected,
                $"Type expected."
            );
        }

        public static CompilerMessage UnexpectedToken (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.UnexpectedToken,
                $"Unexpected Token: '{token.Kind}'."
            );
        }

        public static CompilerMessage RightParenExpected () {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.RightParenExpected,
                $"')' expected."
            );
        }

        public static CompilerMessage InvalidTopLevelStatement () {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.InvalidTopLevelStatement,
                $"Invalid top-level statement."
            );
        }
    }
}
