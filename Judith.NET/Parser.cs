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
                if (IsAtEnd()) break;

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
        else {
            return Statement();
        }

        //throw Error(CompilerMessage.Parser.InvalidTopLevelStatement());
    }

    private LocalDeclarationStatement LocalDeclarationStatement (
        FieldKind fieldKind
    ) {
        FieldDeclarationExpression fieldDecl = FieldDeclarationExpression(fieldKind);
        
        return sf.LocalDeclarationStatement(fieldDecl);
    }

    /// <summary>
    /// Parses a field declaration that may be implicit (i.e. not qualified with
    /// either "const" or "var"). Unlike <see cref="FieldDeclarationExpression(FieldKind)"/>,
    /// this method assumes that the mutability keyword, if it exists, has not
    /// been consumed yet.
    /// </summary>
    /// <returns></returns>
    private FieldDeclarationExpression ImplicitFieldDeclarationExpression () {
        Token? mutabilityToken = Peek();
        if (mutabilityToken.Kind == TokenKind.Identifier) {
            return FieldDeclarationExpression(FieldKind.Constant);
        }
        if (mutabilityToken.Kind == TokenKind.KwConst) {
            Advance();
            return FieldDeclarationExpression(FieldKind.Constant);
        }
        if (mutabilityToken.Kind == TokenKind.KwVar) {
            Advance();
            return FieldDeclarationExpression(FieldKind.Variable);
        }

        throw Error(CompilerMessage.Parser.FieldDeclarationExpected(mutabilityToken.Line));
    }

    private FieldDeclarationExpression FieldDeclarationExpression (
        FieldKind fieldKind
    ) {
        Token? fieldKindToken = PeekPrevious();
        // If the last keyword is not const or var, then this is an implicit
        // "const" and there's no token for it.
        if (fieldKindToken.Kind != TokenKind.KwConst && fieldKindToken.Kind != TokenKind.KwVar) {
            fieldKindToken = null;
        }

        // This is the beginning of the declaration, so the next token must be
        // an identifier. We just check it's there, FieldDeclarator() will be
        // the one consuming it.
        if (Peek().Kind != TokenKind.Identifier) {
            throw Error(CompilerMessage.Parser.IdentifierExpected(Peek().Line));
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

        EqualsValueClause? initializer = null;

        if (Match(TokenKind.Equal)) {
            Token equalsToken = PeekPrevious();
            Expression expr = Expression();

            if (expr is null) {
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
            }

            initializer = sf.EqualsValueClause(expr, equalsToken);
        }

        if (declarators.Count == 1) {
            return sf.SingleFieldDeclarationExpression(declarators[0], initializer);
        }
        else {
            return sf.MultipleFieldDeclarationExpression(declarators, initializer);
        }
    }

    private Statement Statement () {
        if (Match(TokenKind.KwReturn)) {
            return ReturnStatement();
        }
        else if (Match(TokenKind.KwYield)) {
            return YieldStatement();
        }
        else {
            return ExpressionStatement();
        }
    }

    private BodyStatement BlockOrArrowStatement (TokenKind? openingToken) {
        if (Match(TokenKind.EqualArrow)) {
            return ArrowStatement();
        }
        else if (openingToken == null || Match(openingToken.Value)) {
            return BlockStatement();
        }
        else {
            throw Error(CompilerMessage.Parser.BlockOpeningKeywordExpected(
                Peek().Line, Token.GetTokenName(openingToken.Value), Peek()
            ));
        }
    }

    private BlockStatement BlockStatement () {
        List<Statement> statements = new();

        Token openingToken = PeekPrevious();
        while (MatchBlockEndingToken() == false && IsAtEnd() == false) {
            var stmt = TopLevelStatement();
            if (stmt is not null) {
                statements.Add(stmt);
            }
        }
        Token closingToken = PeekPrevious();

        return sf.BlockStatement(openingToken, statements, closingToken);
    }

    private ArrowStatement ArrowStatement () {
        Token arrowToken = PeekPrevious();
        if (arrowToken.Kind != TokenKind.EqualArrow) throw Error(
            CompilerMessage.Parser.ArrowExpected(arrowToken.Line, arrowToken)
        );

        Statement stmt = Statement();

        return sf.ArrowStatement(arrowToken, stmt);
    }

    public ReturnStatement ReturnStatement () {
        Token returnToken = PeekPrevious();
        Expression expr = Expression();

        return sf.ReturnStatement(returnToken, expr);
    }

    public YieldStatement YieldStatement () {
        Token yieldToken = PeekPrevious();
        Expression expr = Expression();

        return sf.YieldStatement(yieldToken, expr);
    }

    private ExpressionStatement ExpressionStatement () {
        var expr = Expression();

        return sf.ExpressionStatement(expr);
    }

    private Expression Expression () {
        if (Match(TokenKind.KwIf)) {
            return IfExpression();
        }
        else if (Match(TokenKind.KwMatch)) {
            return MatchExpression();
        }
        else if (Match(TokenKind.KwLoop)) {
            return LoopExpression();
        }
        else if (Match(TokenKind.KwWhile)) {
            return WhileExpression();
        }
        else if (Match(TokenKind.KwFor)) {
            return ForeachExpression();
        }
        return AssignmentExpression();
    }

    private IfExpression IfExpression () {
        Token ifToken = PeekPrevious();
        Expression test = Expression();

        BodyStatement consequent = BlockOrArrowStatement(TokenKind.KwThen);

        if (consequent.Kind == SyntaxKind.BlockStatement) {
            BlockStatement block = (BlockStatement)consequent;
            if (block.ClosingToken == null) {
                throw new Exception(
                    "Block consequent in IfStatement() needs an closing token."
                );
            }

            if (block.ClosingToken.Kind == TokenKind.KwElsif) return _Elsif();
            if (block.ClosingToken.Kind == TokenKind.KwElse) return _Else();
            return _If();
        }
        else if (consequent.Kind == SyntaxKind.ArrowStatement) {
            if (Match(TokenKind.KwElsif)) return _Elsif();
            if (Match(TokenKind.KwElse)) return _Else();
            return _If();
        }
        else {
            throw new Exception($"Invalid consequent kind: {consequent.Kind}");
        }

        IfExpression _Elsif () {
            var elsifToken = PeekPrevious();
            var alternate = IfExpression();
            var alternateStmt = sf.ExpressionStatement(alternate);
            return sf.IfExpression(ifToken, test, consequent, elsifToken, alternateStmt);
        }

        IfExpression _Else () {
            var elseToken = PeekPrevious();
            var alternate = BlockOrArrowStatement(null);
            return sf.IfExpression(ifToken, test, consequent, elseToken, alternate);
        }

        IfExpression _If () {
            return sf.IfExpression(ifToken, test, consequent);
        }
    }

    public MatchExpression MatchExpression () {
        var matchToken = PeekPrevious();
        var discriminant = Expression();

        if (TryConsume(TokenKind.KwDo, out Token? doToken) == false) {
            throw Error(CompilerMessage.Parser.DoExpected(Peek().Line));
        }

        List<MatchCase> cases = new();
        while (CheckLiteral() || Check(TokenKind.KwElse)) {
            cases.Add(MatchCase());
        }

        if (TryConsume(TokenKind.KwEnd, out Token? endToken) == false) {
            throw Error(CompilerMessage.Parser.EndExpected(Peek().Line));
        }

        return sf.MatchExpression(matchToken, discriminant, doToken, cases, endToken);
    }

    public MatchCase MatchCase () {
        List<Expression> tests = new();

        // If "else" is matched, this is the default case and we don't need to
        // try to get patterns. If it doesn't, then one or more patterns
        // (separated by commas) must appear next.
        if (TryConsume(TokenKind.KwElse, out Token? elseToken) == false) {
            while (CheckLiteral()) {
                LiteralExpression? literal = LiteralExpression();
                if (literal is not null) tests.Add(literal);

                // After each literal, there must be a comma for a subsequent
                // literal to be a valid token.
                if (Match(TokenKind.Comma) == false) {
                    break;
                }
            }
        }

        var consequent = BlockOrArrowStatement(elseToken == null ? TokenKind.KwDo : null);

        return sf.MatchCase(elseToken, tests, consequent);
    }

    public LoopExpression LoopExpression () {
        Token loopToken = PeekPrevious();

        BodyStatement body = BlockOrArrowStatement(null);

        return sf.LoopExpression(loopToken, body);
    }

    public WhileExpression WhileExpression () {
        Token whileToken = PeekPrevious();
        Expression test = Expression();

        BodyStatement body = BlockOrArrowStatement(TokenKind.KwDo);

        return sf.WhileExpression(whileToken, test, body);
    }

    public ForeachExpression ForeachExpression () {
        Token foreachToken = PeekPrevious();
        FieldDeclarationExpression initializer = ImplicitFieldDeclarationExpression();

        if (TryConsume(TokenKind.KwIn, out Token? inToken) == false) {
            throw Error(CompilerMessage.Parser.InExpected(Peek().Line));
        }

        Expression enumerable = Expression();
        BodyStatement body = BlockOrArrowStatement(TokenKind.KwDo);

        return sf.ForeachExpression(foreachToken, initializer, inToken, enumerable, body);
    }

    // "="
    private Expression AssignmentExpression () {
        Expression expr = LogicalBinaryExpression();

        if (Match(TokenKind.Equal)) {
            Token equalToken = PeekPrevious();
            var right = AssignmentExpression();

            return sf.AssignmentExpression(expr, equalToken, right);
        }

        return expr;
    }

    // "and" | "or"
    private Expression LogicalBinaryExpression () {
        Expression expr = ComparisonBinaryExpression();

        while (MatchLogicalToken()) {
            Token op = PeekPrevious();
            Expression right = ComparisonBinaryExpression();

            expr = sf.BinaryExpression(expr, sf.Operator(op), right);
        }

        return expr;
    }

    // "==" | "!=" | "<" | "<=" | ">" | ">="
    private Expression ComparisonBinaryExpression () {
        Expression expr = AdditionBinaryExpression();

        while (MatchComparisonToken()) {
            Token op = PeekPrevious();
            Expression right = AdditionBinaryExpression();

            expr = sf.BinaryExpression(expr, sf.Operator(op), right);
        }

        return expr;
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

    // "not"
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
                throw Error(CompilerMessage.Parser.RightParenExpected(Peek().Line));
            }
            
            return sf.GroupExpression(expr, leftParen, rightParen);
        }

        IdentifierExpression? identifier = IdentifierExpression();
        if (identifier is not null) {
            return identifier;
        }

        throw Error(CompilerMessage.Parser.UnexpectedToken(Peek().Line, Advance()));
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
        if (Match(TokenKind.String)) {
            return sf.Literal(token);
        }

        return null;
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
            throw Error(CompilerMessage.Parser.IdentifierExpected(Peek().Line));
        }

        IdentifierExpression? type = null;
        if (Match(TokenKind.Colon)) {
            type = IdentifierExpression();

            if (type == null) {
                throw Error(CompilerMessage.Parser.TypeExpected(Peek().Line));
            }
        }

        return sf.FieldDeclarator(
            fieldKindToken,
            sf.Identifier(identifierToken),
            fieldKind,
            type
        );
    }
    #endregion

    #region Token matching
    private bool MatchBlockEndingToken () {
        return Match(
            TokenKind.KwEnd,
            TokenKind.KwElse,
            TokenKind.KwElsif
        );
    }

    private bool MatchLogicalToken () {
        return Match(
            TokenKind.KwAnd,
            TokenKind.KwOr
        );
    }

    private bool MatchComparisonToken () {
        return Match(
            TokenKind.EqualEqual,
            TokenKind.BangEqual,
            TokenKind.Less,
            TokenKind.LessEqual,
            TokenKind.Greater,
            TokenKind.GreaterEqual
        );
    }

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

    private bool CheckLiteral () {
        return Check(
            TokenKind.Number,
            TokenKind.String
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