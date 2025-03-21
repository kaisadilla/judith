using Judith.NET.analysis.lexical;
using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Judith.NET.message;

[JsonConverter(typeof(StringEnumConverter))]
public enum MessageKind {
    Information,
    Warning,
    Error,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum MessageOrigin {
    Lexer,
    Parser,
    SymbolTableBuilder,
    SymbolResolver,
    Binder,
    TypeResolver,
    BlockTypeResolver,
    ImplicitNodeAnalyzer,
    TypeAnalyzer,
}

public class CompilerMessage {
    public MessageKind Kind { get; init; }
    public MessageOrigin Origin { get; init; }
    public int Code { get; init; }
    public string Message { get; init; }
    public MessageSource Source { get; init; }

    private CompilerMessage (
        MessageKind kind,
        MessageOrigin origin,
        int code,
        string message,
        MessageSource source
    ) {
        Kind = kind;
        Origin = origin;
        Code = code;
        Message = message;
        Source = source;
    }

    public override string ToString () {
        return $"[{Origin}/{Kind}] {Code,4} - {Message} (Line {Source.GetLine()})";
    }

    public string GetElaborateMessage (string? src = null) {
        string location;

        if (Source.AsLine.HasValue) {
            location = "line: " + Source.AsLine.Value;
        }
        else if (Source.AsToken != null) {
            location = $"line: {Source.AsToken.Line} (at '{Source.AsToken.Lexeme}')";
        }
        else if (Source.AsNode != null) {
            if (src != null) {
                string snippet = src.Substring(
                    Source.AsNode.Span.Start, Source.AsNode.Span.Length
                );

                location = $"line: {Source.AsNode.Line} (around '{snippet}')";
            }
            else {
                location = $"line: {Source.AsNode.Line} (in '{Source.AsNode}')";
            }
        }
        else {
            throw new InvalidUnionException();
        }

        return $"[{Origin} / {Kind}] {Code,4} - {Message} \n  - at {location}";
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
                new(line)
            );
        }

        public static CompilerMessage UnterminatedString (int line) {
            return new(
                MessageKind.Error,
                MessageOrigin.Lexer,
                (int)MessageCode.UnterminatedString,
                $"Unterminated string.",
                new(line)
            );
        }
    }

    public static class Parser {
        public static CompilerMessage IdentifierExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.IdentifierExpected,
                $"Expected identifier, found '{token.Kind}'",
                new(token)
            );
        }

        public static CompilerMessage TypeExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.TypeExpected,
                $"Expected type, found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage TypeAnnotationExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.TypeAnnotationExpected,
                $"Expected type annotation, found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage UnexpectedToken (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.UnexpectedToken,
                $"Unexpected Token: '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage LeftParenExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.LeftParenExpected,
                $"Expected '(', found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage RightParenExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.RightParenExpected,
                $"Expected ')', found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage RightParenExpected (SyntaxNode node) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.RightParenExpected,
                $"Expected ')', found '{node.Kind}'.",
                new(node)
            );
        }

        public static CompilerMessage RightCurlyBracketExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.RightCurlyBracketExpected,
                $"Expected '}}', found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage RightSquareBracketExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.RightSquareBracketExpected,
                $"Expected ']', found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage ExpressionExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.ExpressionExpected,
                $"Expected expression, found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage StatementExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.StatementExpected,
                $"Expected statement, found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage BlockOpeningKeywordExpected (
            Token token, string keyword
        ) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.BlockOpeningKeywordExpected,
                $"Expected '{keyword}', found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage BodyExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.BodyExpected,
                $"Expected body, found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage ArrowExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.ArrowExpected,
                $"Expected '=>', found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage ElsifBodyExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.ElsifBodyExpected,
                $"Expected elsif body, found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage InExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.InExpected,
                $"Expected 'in', found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage DoExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.DoExpected,
                $"Expected 'do', found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage EndExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.EndExpected,
                $"Expected 'end', found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage ParameterExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.ParameterExpected,
                $"Expected parameter, found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage ArgumentExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.ArgumentExpected,
                $"Expected argument, found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage HidableItemExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.HidableItemExpected,
                $"Expected top-level item, found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage LocalDeclaratorExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.LocalDeclaratorExpected,
                $"Expected local declarator, found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage LocalDeclaratorListExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.LocalDeclaratorListExpected,
                $"Expected local declarator list, found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage AssignmentExpressionExpected (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.AssignmentExpressionExpected,
                $"Expected assignment, found '{token.Kind}'.",
                new(token)
            );
        }

        public static CompilerMessage InvalidTopLevelStatement (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.InvalidTopLevelStatement,
                $"Invalid top-level statement.",
                new(token)
            );
        }

        public static CompilerMessage InvalidIntegerLiteral (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.InvalidIntegerLiteral,
                $"Invalid integer literal.",
                new(token)
            );
        }

        public static CompilerMessage InvalidFloatLiteral (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.InvalidFloatLiteral,
                $"Invalid float literal.",
                new(token)
            );
        }

        public static CompilerMessage ParameterTypeMustBeSpecified (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.ParameterTypeMustBeSpecified,
                $"Parameters must specify their type.",
                new(token)
            );
        }

        public static CompilerMessage FieldMustBeInitialized (Token token) {
            return new(
                MessageKind.Error,
                MessageOrigin.Parser,
                (int)MessageCode.FieldMustBeInitialized,
                $"Field must be initialized with a value.",
                new(token)
            );
        }
    }

    public static class Analyzers {
        public static CompilerMessage DefinitionAlreadyExist (
            SyntaxNode node, string name
        ) {
            return new(
                MessageKind.Error,
                MessageOrigin.SymbolTableBuilder,
                (int)MessageCode.DefinitionAlreadyExist,
                $"The name '{name}' already exists in the current context.",
                new(node)
            );
        }

        public static CompilerMessage NameDoesNotExist (SyntaxNode node, string name) {
            return new(
                MessageKind.Error,
                MessageOrigin.SymbolResolver,
                (int)MessageCode.NameDoesNotExist,
                $"The name '{name}' does not exist in the current context.",
                new(node)
            );
        }

        public static CompilerMessage NameIsAmbiguous (SyntaxNode node, string name) {
            return new(
                MessageKind.Error,
                MessageOrigin.SymbolResolver,
                (int)MessageCode.NameDoesNotExist,
                $"The name '{name}' is ambiguous.",
                new(node)
            );
        }

        public static CompilerMessage NumberSuffixCannotBeUsedForDecimal (
            SyntaxNode node, string suffix
        ) {
            return new(
                MessageKind.Error,
                MessageOrigin.Binder,
                (int)MessageCode.NumberSuffixCannotBeUsedForDecimal,
                $"Suffix '{suffix}' cannot be used for decimal number literal.",
                new(node)
            );
        }

        public static CompilerMessage FloatLiteralOverflow (
            SyntaxNode node, string literalStr, string type
        ) {
            return new(
                MessageKind.Error,
                MessageOrigin.Binder,
                (int)MessageCode.FloatLiteralOverflow,
                $"Floating-point literal '{literalStr}' is outside the range of type '{type}'.",
                new(node)
            );
        }

        public static CompilerMessage IntegerLiteralOverflow (
            SyntaxNode node, string literalStr, string type
        ) {
            return new(
                MessageKind.Error,
                MessageOrigin.Binder,
                (int)MessageCode.IntegerLiteralOverflow,
                $"Signed integer literal '{literalStr}' is outside the range of type '{type}'.",
                new(node)
            );
        }

        public static CompilerMessage UnsignedIntegerLiteralOverflow (
            SyntaxNode node, string literalStr, string type
        ) {
            return new(
                MessageKind.Error,
                MessageOrigin.Binder,
                (int)MessageCode.UnsignedIntegerLiteralOverflow,
                $"Unsigned integer literal '{literalStr}' is outside the range of type '{type}'.",
                new(node)
            );
        }

        public static CompilerMessage IntegerLiteralIsTooLarge (SyntaxNode node) {
            return new(
                MessageKind.Error,
                MessageOrigin.Binder,
                (int)MessageCode.IntegerLiteralIsTooLarge,
                $"Integer literal is too large.",
                new(node)
            );
        }

        public static CompilerMessage UndefinedBinaryOperation (
            SyntaxNode node, string type1, string type2, string op
        ) {
            return new(
                MessageKind.Error,
                MessageOrigin.Binder,
                (int)MessageCode.UndefinedBinaryOperation,
                $"Operator '{op}' cannot be applied to types '{type1}' and '{type2}'.",
                new(node)
            );
        }

        public static CompilerMessage TypeDoesntExist (SyntaxNode node, string type) {
            return new(
                MessageKind.Error,
                MessageOrigin.TypeResolver,
                (int)MessageCode.TypeDoesntExist,
                $"The type '{type}' does not exist in the current context.",
                new(node)
            );
        }

        public static CompilerMessage TypeExpectedTR (SyntaxNode node) {
            return new(
                MessageKind.Error,
                MessageOrigin.TypeResolver,
                (int)MessageCode.TypeExpectedTR,
                $"Type identifier expected.",
                new(node)
            );
        }

        public static CompilerMessage InvalidTypeForObjectInitialization (
            SyntaxNode node, string fqn
        ) {
            return new(
                MessageKind.Error,
                MessageOrigin.TypeResolver,
                (int)MessageCode.InvalidTypeForObjectInitialization,
                $"Cannot initialize an object with type '{fqn}'.",
                new(node)
            );
        }

        public static CompilerMessage MemberAccessOnlyOnInstances (SyntaxNode node) {
            return new(
                MessageKind.Error,
                MessageOrigin.TypeResolver,
                (int)MessageCode.MemberAccessOnlyOnInstances,
                $"Only instances of a type can be accessed with '.' operator.",
                new(node)
            );
        }

        public static CompilerMessage ScopeAccessNotOnInstances (SyntaxNode node) {
            return new(
                MessageKind.Error,
                MessageOrigin.TypeResolver,
                (int)MessageCode.MemberAccessOnlyOnInstances,
                $"Scope resolution operator ('::') cannot be used on instances of a type.",
                new(node)
            );
        }

        public static CompilerMessage FieldDoesNotExist (
            SyntaxNode node, string type, string member
        ) {
            return new(
                MessageKind.Error,
                MessageOrigin.TypeResolver,
                (int)MessageCode.FieldDoesNotExist,
                $"Type '{type}' does not contain an instance member named '{member}'.",
                new(node)
            );
        }

        public static CompilerMessage InconsistentReturnBehavior (SyntaxNode node) {
            return new(
                MessageKind.Error,
                MessageOrigin.BlockTypeResolver,
                (int)MessageCode.InconsistentReturnBehavior,
                "Inconsistent return behavior. Some paths return values while " +
                "others return Void. This is not allowed.",
                new(node)
            );
        }

        public static CompilerMessage ReturnNotAllowed (SyntaxNode node) {
            return new(
                MessageKind.Error,
                MessageOrigin.ImplicitNodeAnalyzer,
                (int)MessageCode.ReturnNotAllowed,
                "Return statements are not allowed in this context.",
                new(node)
            );
        }

        public static CompilerMessage YieldNotAllowed (SyntaxNode node) {
            return new(
                MessageKind.Error,
                MessageOrigin.ImplicitNodeAnalyzer,
                (int)MessageCode.YieldNotAllowed,
                "Yield statements are not allowed in this context.",
                new(node)
            );
        }

        public static CompilerMessage NotAllPathsYieldValue (SyntaxNode node) {
            return new(
                MessageKind.Error,
                MessageOrigin.ImplicitNodeAnalyzer,
                (int)MessageCode.NotAllPathsYieldValue,
                "Not all paths yield a value.",
                new(node)
            );
        }

        public static CompilerMessage CannotAssignType (
            SyntaxNode node, TypeSymbol target, TypeSymbol value
        ) {
            return new(
                MessageKind.Error,
                MessageOrigin.TypeAnalyzer,
                (int)MessageCode.CannotAssignType,
                $"Cannot assign type '{value.FullyQualifiedName}' to target of " +
                $"type '{target.FullyQualifiedName}'.",
                new(node)
            );
        }
    }
}
