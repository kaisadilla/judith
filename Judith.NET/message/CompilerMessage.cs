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

        public static CompilerMessage RightCurlyBracketExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.RightCurlyBracketExpected,
                "'}' expected.",
                line
            );
        }

        public static CompilerMessage RightSquareBracketExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.RightSquareBracketExpected,
                $"']' expected.",
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

        public static CompilerMessage StatementExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.StatementExpected,
                $"Statement expected.",
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

        public static CompilerMessage BodyExpected (
            int line
        ) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.BodyExpected,
                $"Body expected.",
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

        public static CompilerMessage ElsifBodyExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.ElsifBodyExpected,
                $"Elsif body expected.",
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

        public static CompilerMessage HidableItemExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.HidableItemExpected,
                $"Function, generator, typedef, symbol or enumerate expected.",
                line
            );
        }

        public static CompilerMessage LocalDeclaratorExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.LocalDeclaratorExpected,
                $"Local declarator expected.",
                line
            );
        }

        public static CompilerMessage LocalDeclaratorListExpected (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.LocalDeclaratorListExpected,
                $"Local declarator list expected.",
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

        public static CompilerMessage InvalidIntegerLiteral (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.InvalidIntegerLiteral,
                $"Invalid integer literal.",
                line
            );
        }

        public static CompilerMessage InvalidFloatLiteral (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.InvalidFloatLiteral,
                $"Invalid float literal.",
                line
            );
        }
    }
}
