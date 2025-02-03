using Judith.NET.message;
using Judith.NET.syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using sf = Judith.NET.syntax.SyntaxFactory;

namespace Judith.NET;

public class Parser {
    public MessageContainer? Messages { get; private set; }
    public bool HasError { get; private set; } = false;

    private readonly List<Token> _tokens;
    private int _cursor = 0;

    public List<SyntaxNode>? Nodes = null;

    public Parser (List<Token> tokens, MessageContainer? messages = null) {
        _tokens = tokens;
        Messages = messages;
    }

    public void Parse () {
        Nodes = new();

        while (IsAtEnd() == false) {
            try {
                SkipComments();

                SyntaxNode? node = TopLevelStatement();

                if (node is not null) {
                    Nodes.Add(node);
                }
            }
            catch (ParseException ex) {
                // Recovery
                Advance();
            }
        }
    }

    private void SkipComments () {
        while (IsAtEnd() == false && Peek().Kind == TokenKind.Comment) {
            Advance();
        }
    }

    #region Helper methods
    /// <summary>
    /// Returns true if the current token matches the kind given. In this
    /// case, consumes said token.
    /// </summary>
    /// <param name="kinds">The kinds to match.</param>
    private bool Match (params TokenKind[] kinds) {
        foreach (var kind in kinds) {
            if (Check(kind)) {
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool Match (out Token? token, params TokenKind[] kinds) {
        if (Match(kinds)) {
            token = PeekPrevious();
            return true;
        }

        token = null;
        return false;
    }

    /// <summary>
    /// Retruns true if the current token matches the type given.
    /// </summary>
    /// <param name="kind">The token type to check for.</param>
    private bool Check (TokenKind kind) {
        if (IsAtEnd()) return false;

        return Peek().Kind == kind;
    }

    private bool Check (params TokenKind[] kinds) {
        foreach (var kind in kinds) {
            if (Check(kind)) return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if the current token is the EOF token.
    /// </summary>
    private bool IsAtEnd () {
        return Peek().Kind == TokenKind.EOF;
    }

    /// <summary>
    /// Returns the current token.
    /// </summary>
    private Token Peek () {
        return _tokens[_cursor];
    }

    /// <summary>
    /// Returns the previous token.
    /// </summary>
    private Token PeekPrevious () {
        return _tokens[_cursor - 1];
    }

    /// <summary>
    /// Returns the current token and moves the cursor to the next one.
    /// </summary>
    /// <returns></returns>
    private Token Advance () {
        if (IsAtEnd() == false) _cursor++;

        return PeekPrevious();
    }

    /// <summary>
    /// Returns whether the current token matches the token kind given and, if
    /// it does, outputs it to the 'token' parameter.
    /// </summary>
    /// <param name="kind">The kind ot token to try to match.</param>
    /// <param name="token">The token matched, if any.</param>
    /// <returns></returns>
    private bool TryConsume (TokenKind kind, [NotNullWhen(true)] out Token? token) {
        if (Check(kind)) {
            token = Advance();
            return true;
        }

        token = null;
        return false;
    }
    #endregion

    #region Parsing methods
    private Statement TopLevelStatement () {
        if (Match(TokenKind.KwConst)) {
            return LocalDeclarationStatement(FieldKind.Constant);
        }
        else if (Match(TokenKind.KwVar)) {
            return LocalDeclarationStatement(FieldKind.Variable);
        }

        throw Error(CompilerMessage.Parser.InvalidTopLevelStatement());
    }

    private LocalDeclarationStatement LocalDeclarationStatement (
        FieldKind fieldKind
    ) {
        FieldDeclarationExpression fieldDecl = FieldDeclarationExpression(fieldKind);
        
        return sf.LocalDeclarationStatement(fieldDecl);
    }

    private FieldDeclarationExpression FieldDeclarationExpression (
        FieldKind fieldKind
    ) {
        Token? fieldKindToken = PeekPrevious();

        // This is the beginning of the declaration, so the next token must be
        // an identifier. We just check it's there, FieldDeclarator() will be
        // the one consuming it.
        if (Peek().Kind != TokenKind.Identifier) {
            throw Error(CompilerMessage.Parser.IdentifierExpected());
        }

        List<FieldDeclarator> declarators = new();

        do {
            var declarator = FieldDeclarator(fieldKind, fieldKindToken);
            declarators.Add(declarator);
            fieldKind = declarator.FieldKind;
            fieldKindToken = null;
        }
        while (Match(TokenKind.Comma));

        // Because types work right to left, we can't start assigning types to
        // declarators without types until we have parsed all of them.
        // Note that it is possible for the last declarator not to specify a
        // type, in which case it will propagate "null" as its type.
        // This is valid behavior, as type may be inferred from usage.
        for (int i = declarators.Count - 2; i >= 0; i--) {
            if (declarators[i].Type is null) {
                declarators[i].SetType(declarators[i + 1].Type);
            }
        }

        if (declarators.Count == 1) {
            return sf.SingleFieldDeclarationExpression(declarators[0]);
        }
        else {
            return sf.MultipleFieldDeclarationExpression(declarators);
        }
    }

    private Expression Expression () {
        return AdditionBinaryExpression();
    }

    private FieldDeclarator FieldDeclarator (FieldKind fieldKind, Token? fieldKindToken) {
        // If we find a "const" or "var", that's the new mutability.
        if (Match(TokenKind.KwConst)) {
            fieldKindToken = PeekPrevious();
            fieldKind = FieldKind.Constant;
        }
        else if (Match(TokenKind.KwVar)) {
            fieldKindToken = PeekPrevious();
            fieldKind = FieldKind.Variable;
        }

        if (TryConsume(TokenKind.Identifier, out Token? identifierToken) == false) {
            throw Error(CompilerMessage.Parser.IdentifierExpected());
        }

        IdentifierExpression? type = null;
        if (Match(TokenKind.Colon)) {
            type = IdentifierExpression();

            if (type == null) {
                throw Error(CompilerMessage.Parser.TypeExpected());
            }
        }

        return sf.FieldDeclarator(
            fieldKindToken,
            sf.Identifier(identifierToken),
            fieldKind,
            type
        );
    }

    // "+" | "-"
    private Expression AdditionBinaryExpression () {
        Expression expr = MathBinaryExpression();

        while (MatchAdditionToken()) {
            Token op = PeekPrevious();
            Expression right = MathBinaryExpression();

            expr = sf.BinaryExpression(expr, sf.Operator(op), right);
        }

        return expr;
    }

    // "*" | "/"
    private Expression MathBinaryExpression () {
        Expression expr = LeftUnaryExpression();

        while (MatchMathToken()) {
            Token op = PeekPrevious();
            Expression right = LeftUnaryExpression();

            expr = sf.BinaryExpression(expr, sf.Operator(op), right);
        }

        return expr;
    }

    private Expression LeftUnaryExpression () {
        if (MatchLeftUnaryToken()) {
            Token op = PeekPrevious();
            var right = LeftUnaryExpression();
            return sf.LeftUnaryExpression(sf.Operator(op), right);
        }

        return Primary();
    }

    private Expression Primary () {
        LiteralExpression? literal = LiteralExpression();

        if (literal is not null) {
            return literal;
        }
        else if (Match(TokenKind.LeftParen)) {
            Token leftParen = PeekPrevious();
            Expression expr = Expression();

            if (TryConsume(TokenKind.RightParen, out Token? rightParen) == false) {
                throw Error(CompilerMessage.Parser.RightParenExpected());
            }
            
            return sf.GroupExpression(expr, leftParen, rightParen);
        }

        IdentifierExpression? identifier = IdentifierExpression();
        if (identifier is not null) {
            return identifier;
        }

        throw Error(CompilerMessage.Parser.UnexpectedToken(Advance()));
    }

    private IdentifierExpression? IdentifierExpression () {
        if (Match(TokenKind.Identifier)) {
            Token identifierToken = PeekPrevious();
            var id = sf.Identifier(identifierToken);
            return sf.IdentifierExpression(id);
        }
        
        return null;
    }

    private LiteralExpression? LiteralExpression () {
        Literal? literal = Literal();

        if (literal is not null) {
            return sf.LiteralExpression(literal);
        }

        return null;
    }

    private Literal? Literal () {
        var token = Peek();

        if (Match(TokenKind.KwTrue)) {
            return sf.Literal(token);
        }
        if (Match(TokenKind.KwFalse)) {
            return sf.Literal(token);
        }
        if (Match(TokenKind.Number)) {
            return sf.Literal(token);
        }
        else {
            return null;
        }
    }
    #endregion

    #region Token matching
    private bool MatchAdditionToken () {
        return Match(
            TokenKind.Plus,
            TokenKind.Minus
        );
    }

    private bool MatchMathToken () {
        return Match(
            TokenKind.Asterisk,
            TokenKind.Slash
        );
    }

    private bool MatchLeftUnaryToken () {
        return Match(
            TokenKind.KwNot,
            TokenKind.Minus
        );
    }
    #endregion

    /// <summary>
    /// Registers an error while parsing tokens, setting the error flag and
    /// adding it to the compiler message queue if it exists.
    /// </summary>
    /// <param name="error">The error to add.</param>
    private ParseException Error (CompilerMessage error) {
        HasError = true;
        Messages?.Add(error);

        return new(error);
    }
}

public class ParseException : Exception {
    public CompilerMessage CompilerMessage { get; private set; }

    public ParseException (CompilerMessage message) : base(message.Message) {
        CompilerMessage = message;
    }
}