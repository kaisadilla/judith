using Judith.NET.syntax;
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
    public int Line { get; init; }
    //public SourceSpan ErrorSpan { get; init; }

    private CompilerMessage (
        MessageKind kind,
        MessageOrigin origin,
        int code,
        string message,
        int line
    ) {
        Kind = kind;
        Origin = origin;
        Code = code;
        Message = message;
        Line = line;
    }

    public override string ToString () {
        return $"[{Origin}/{Kind}] {Code,4} - {Message} (Line {Line})";
    }

    public static class Lexer {
        public static CompilerMessage UnexpectedCharacter (
            int line, char unexpectedChar
        ) {
            return new(
                MessageKind.Error,
                MessageOrigin.Lexer,
                (int)MessageCode.UnexpectedCharacter,
                $"Unexpected character: '{unexpectedChar}'.",
                line
            );
        }

        public static CompilerMessage UnterminatedString (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Lexer,
                (int)MessageCode.UnterminatedString,
                $"Unterminated string.",
                line
            );
        }
    }

    public static class Parser {
        public static CompilerMessage IdentifierExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.IdentifierExpected,
                $"Identifier expected.",
                line
            );
        }

        public static CompilerMessage TypeExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.TypeExpected,
                $"Type expected.",
                line
            );
        }

        public static CompilerMessage UnexpectedToken (int line, Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.UnexpectedToken,
                $"Unexpected Token: '{token.Kind}'.",
                line
            );
        }

        public static CompilerMessage LeftParenExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.LeftParenExpected,
                $"'(' expected.",
                line
            );
        }

        public static CompilerMessage RightParenExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.RightParenExpected,
                $"')' expected.",
                line
            );
        }

        public static CompilerMessage ExpressionExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.ExpressionExpected,
                $"Expression expected.",
                line
            );
        }

        public static CompilerMessage FieldDeclarationExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.FieldDeclarationExpected,
                $"Field declaration expected.",
                line
            );
        }

        public static CompilerMessage BlockOpeningKeywordExpected (
            int line, string keyword, Token found
        ) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.BlockOpeningKeywordExpected,
                $"'{keyword}' expected, but '{found.Lexeme}' found instead.",
                line
            );
        }

        public static CompilerMessage ArrowExpected (int line, Token found) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.ArrowExpected,
                $"'Arrow' expected, but '{found.Lexeme}' found instead.",
                line
            );
        }

        public static CompilerMessage InExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.InExpected,
                $"'In' expected.",
                line
            );
        }

        public static CompilerMessage DoExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.DoExpected,
                $"'Do' expected.",
                line
            );
        }

        public static CompilerMessage EndExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.EndExpected,
                $"'End' expected.",
                line
            );
        }

        public static CompilerMessage ParameterExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.ParameterExpected,
                $"Parameter expected.",
                line
            );
        }

        public static CompilerMessage InvalidTopLevelStatement (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.InvalidTopLevelStatement,
                $"Invalid top-level statement.",
                line
            );
        }
    }
}
