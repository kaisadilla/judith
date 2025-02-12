using Judith.NET.message;
using Judith.NET.syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SF = Judith.NET.syntax.SyntaxFactory;

namespace Judith.NET;

public class Parser {
    public MessageContainer? Messages { get; private set; }
    public bool HasError { get; private set; } = false;

    private readonly List<Token> _tokens;
    private int _cursor = 0;

    public List<SyntaxNode>? Nodes { get; private set; } = null;

    public Parser (List<Token> tokens, MessageContainer? messages = null) {
        _tokens = tokens.Where(t => t.Kind != TokenKind.Comment).ToList(); // TODO: incorporate comments, whitespaces and enters to tokens as trivia in the lexer.
        Messages = messages;
    }

    [MemberNotNull(nameof(Nodes))]
    public void Parse () {
        Nodes = new();

        while (IsAtEnd() == false) {
            try {
                if (IsAtEnd()) break;

                SyntaxNode? node = TopLevelNode();

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
    private SyntaxNode TopLevelNode () {
        if (Match(TokenKind.KwFunc)) {
            return FunctionItem();
        }
        // TODO: Scaffolding - remove.
        else if (Match(TokenKind.KwReturn)) {
            return ReturnStatement();
        }

        return Statement();
    }

    private FunctionItem FunctionItem () {
        Token funcToken = PeekPrevious();

        if (TryConsume(TokenKind.Identifier, out Token? identifierToken) == false) {
            throw Error(CompilerMessage.Parser.IdentifierExpected(Peek().Line));
        }

        ParameterList parameters = ParameterList();
        IdentifierExpression? returnType = OptionalTypeAnnotation();
        BodyStatement body = BlockStatement(); // TODO: Decide if we'll allow arrow bodies.

        return SF.FunctionItem(
            funcToken,
            SF.Identifier(identifierToken),
            parameters,
            returnType,
            body
        );
    }

    private ParameterList ParameterList () {
        if (TryConsume(TokenKind.LeftParen, out Token? leftParenToken) == false) {
            throw Error(CompilerMessage.Parser.LeftParenExpected(Peek().Line));
        }

        List<Parameter> parameters = new();
        if (Check(TokenKind.RightParen) == false) {
            do {
                FieldDeclarationExpression decl = ImplicitFieldDeclarationExpression();
                var paramsInDecl = ExtractParamsFromFieldDeclaration(decl);
                parameters.AddRange(paramsInDecl);
            }
            while (Match(TokenKind.Comma) && Peek().Kind != TokenKind.RightParen);
        }

        if (TryConsume(TokenKind.RightParen, out Token? rightParenToken) == false) {
            throw Error(CompilerMessage.Parser.RightParenExpected(Peek().Line));
        }

        return SF.ParameterList(leftParenToken, parameters, rightParenToken);
    }

    private List<Parameter> ExtractParamsFromFieldDeclaration (
        FieldDeclarationExpression expr
    ) {
        List<Parameter> parameters = new();
        if (expr.Kind == SyntaxKind.SingleFieldDeclarationExpression) {
            var decl = (SingleFieldDeclarationExpression)expr;
            parameters.Add(SF.Parameter(
                decl.Declarator.FieldKindToken,
                decl.Declarator.Identifier,
                decl.Declarator.FieldKind,
                decl.Declarator.Type,
                decl.Initializer
            ));
        }
        else if (expr.Kind == SyntaxKind.MultipleFieldDeclarationExpression) {
            var decl = (MultipleFieldDeclarationExpression)expr;

            EqualsValueClause? initializer = null;
            for (int i = 0; i < decl.Declarators.Count; i++) {
                // If this expression has an initializer, in parameter declaration
                // syntax that initializer becomes the default value of the last
                // parameter declared.
                if (i == decl.Declarators.Count - 1) initializer = decl.Initializer;

                parameters.Add(SF.Parameter(
                    decl.Declarators[i].FieldKindToken,
                    decl.Declarators[i].Identifier,
                    decl.Declarators[i].FieldKind,
                    decl.Declarators[i].Type,
                    initializer
                ));
            }
        }
        else {
            throw new Exception(
                "FieldDeclarationExpection doesn't have correct kind."
            );
        }

        return parameters;
    }

    private LocalDeclarationStatement LocalDeclarationStatement (
        FieldKind fieldKind
    ) {
        FieldDeclarationExpression fieldDecl = FieldDeclarationExpression(fieldKind);
        
        return SF.LocalDeclarationStatement(fieldDecl);
    }

    private Statement Statement () {
        if (Match(TokenKind.KwConst)) {
            return LocalDeclarationStatement(FieldKind.Constant);
        }
        else if (Match(TokenKind.KwVar)) {
            return LocalDeclarationStatement(FieldKind.Variable);
        }
        if (Match(TokenKind.KwReturn)) {
            return ReturnStatement();
        }
        else if (Match(TokenKind.KwYield)) {
            return YieldStatement();
        }
        else if (Match(TokenKind.PkwPrint)) {
            return PrivPrintStmt();
        }
        else {
            return ExpressionStatement();
        }
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

            initializer = SF.EqualsValueClause(expr, equalsToken);
        }

        if (declarators.Count == 1) {
            return SF.SingleFieldDeclarationExpression(declarators[0], initializer);
        }
        else {
            return SF.MultipleFieldDeclarationExpression(declarators, initializer);
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
        while (Match(TokenKind.KwEnd) == false && IsAtEnd() == false) {
            var stmt = Statement();
            if (stmt is not null) {
                statements.Add(stmt);
            }
        }
        Token closingToken = PeekPrevious();

        return SF.BlockStatement(openingToken, statements, closingToken);
    }

    private ArrowStatement ArrowStatement () {
        Token arrowToken = PeekPrevious();
        if (arrowToken.Kind != TokenKind.EqualArrow) throw Error(
            CompilerMessage.Parser.ArrowExpected(arrowToken.Line, arrowToken)
        );

        Statement stmt = Statement();

        return SF.ArrowStatement(arrowToken, stmt);
    }

    public ReturnStatement ReturnStatement () {
        Token returnToken = PeekPrevious();
        
        if (Peek().Line != returnToken.Line) {
            return SF.ReturnStatement(returnToken, null);
        }

        Expression expr = Expression();

        return SF.ReturnStatement(returnToken, expr);
    }

    public YieldStatement YieldStatement () {
        Token yieldToken = PeekPrevious();
        Expression expr = Expression();

        return SF.YieldStatement(yieldToken, expr);
    }

    private ExpressionStatement ExpressionStatement () {
        var expr = Expression();

        return SF.ExpressionStatement(expr);
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
            var alternateStmt = SF.ExpressionStatement(alternate);
            return SF.IfExpression(ifToken, test, consequent, elsifToken, alternateStmt);
        }

        IfExpression _Else () {
            var elseToken = PeekPrevious();
            var alternate = BlockOrArrowStatement(null);
            return SF.IfExpression(ifToken, test, consequent, elseToken, alternate);
        }

        IfExpression _If () {
            return SF.IfExpression(ifToken, test, consequent);
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

        return SF.MatchExpression(matchToken, discriminant, doToken, cases, endToken);
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

        return SF.MatchCase(elseToken, tests, consequent);
    }

    public LoopExpression LoopExpression () {
        Token loopToken = PeekPrevious();

        BodyStatement body = BlockOrArrowStatement(null);

        return SF.LoopExpression(loopToken, body);
    }

    public WhileExpression WhileExpression () {
        Token whileToken = PeekPrevious();
        Expression test = Expression();

        BodyStatement body = BlockOrArrowStatement(TokenKind.KwDo);

        return SF.WhileExpression(whileToken, test, body);
    }

    public ForeachExpression ForeachExpression () {
        Token foreachToken = PeekPrevious();
        FieldDeclarationExpression initializer = ImplicitFieldDeclarationExpression();

        if (TryConsume(TokenKind.KwIn, out Token? inToken) == false) {
            throw Error(CompilerMessage.Parser.InExpected(Peek().Line));
        }

        Expression enumerable = Expression();
        BodyStatement body = BlockOrArrowStatement(TokenKind.KwDo);

        return SF.ForeachExpression(foreachToken, initializer, inToken, enumerable, body);
    }

    // "="
    private Expression AssignmentExpression () {
        Expression expr = LogicalBinaryExpression();

        if (Match(TokenKind.Equal)) {
            Token equalToken = PeekPrevious();
            var right = AssignmentExpression();

            return SF.AssignmentExpression(expr, equalToken, right);
        }

        return expr;
    }

    // "and" | "or"
    private Expression LogicalBinaryExpression () {
        Expression expr = ComparisonBinaryExpression();

        while (MatchLogicalToken()) {
            Operator op = Operator();
            Expression right = ComparisonBinaryExpression();

            expr = SF.BinaryExpression(expr, op, right);
        }

        return expr;
    }

    // "==" | "!=" | "<" | "<=" | ">" | ">="
    private Expression ComparisonBinaryExpression () {
        Expression expr = AdditionBinaryExpression();

        while (MatchComparisonToken()) {
            Operator op = Operator();
            Expression right = AdditionBinaryExpression();

            expr = SF.BinaryExpression(expr, op, right);
        }

        return expr;
    }

    // "+" | "-"
    private Expression AdditionBinaryExpression () {
        Expression expr = MathBinaryExpression();

        while (MatchAdditionToken()) {
            Operator op = Operator();
            Expression right = MathBinaryExpression();

            expr = SF.BinaryExpression(expr, op, right);
        }

        return expr;
    }

    // "*" | "/"
    private Expression MathBinaryExpression () {
        Expression expr = LeftUnaryExpression();

        while (MatchMathToken()) {
            Operator op = Operator();
            Expression right = LeftUnaryExpression();

            expr = SF.BinaryExpression(expr, op, right);
        }

        return expr;
    }

    // "not"
    private Expression LeftUnaryExpression () {
        if (MatchLeftUnaryToken()) {
            Operator op = Operator();
            var right = LeftUnaryExpression();
            return SF.LeftUnaryExpression(op, right);
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
            
            return SF.GroupExpression(expr, leftParen, rightParen);
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
            var id = SF.Identifier(identifierToken);
            return SF.IdentifierExpression(id);
        }
        
        return null;
    }

    /// <summary>
    /// Returns an identifier expression used as a type annotation if there's
    /// one, or null if there isn't.
    /// </summary>
    private IdentifierExpression? OptionalTypeAnnotation () {
        if (Match(TokenKind.Colon) == false) return null;

        IdentifierExpression? type = IdentifierExpression();

        if (type == null) throw Error(
            CompilerMessage.Parser.TypeExpected(Peek().Line)
        );

        return type;
    }

    private LiteralExpression? LiteralExpression () {
        Literal? literal = Literal();

        if (literal is not null) {
            return SF.LiteralExpression(literal);
        }

        return null;
    }

    private Literal? Literal () {
        var token = Peek();

        if (Match(TokenKind.KwTrue)) {
            return BooleanLiteral(token);
        }
        if (Match(TokenKind.KwFalse)) {
            return BooleanLiteral(token);
        }
        if (Match(TokenKind.Number)) {
            return NumberLiteral(token);
        }
        if (Match(TokenKind.String)) {
            return SF.Literal(token);
        }

        return null;
    }

    private Literal NumberLiteral (Token token) {
        // The part of the lexeme that represents the number, removing
        // underscores as they don't have any meaning.
        string numberString = token.Lexeme.Replace("_", "");
        // If it contains a dot, it's decimal and will be hold in a double.
        // Same applies if it's expressed in scientific notation: XeY.
        bool isDecimal = numberString.Contains('.') || numberString.Contains('e');

        if (isDecimal) {
            if (double.TryParse(numberString, out double val)) {
                return SF.NumberLiteral(token, val);
            }
            else {
                throw Error(CompilerMessage.Parser.InvalidFloatLiteral(token.Line));
            }
        }
        else {
            if (long.TryParse(numberString, out long val)) {
                return SF.NumberLiteral(token, val);
            }
            else {
                throw Error(CompilerMessage.Parser.InvalidIntegerLiteral(token.Line));
            }
        }
    }

    private Literal BooleanLiteral (Token token) {
        if (token.Kind == TokenKind.KwTrue) {
            return SF.BooleanLiteral(token, true);
        }
        if (token.Kind == TokenKind.KwFalse) {
            return SF.BooleanLiteral(token, false);
        }

        throw new Exception("Trying to parse an invalid token as a boolean.");
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

        IdentifierExpression? type = OptionalTypeAnnotation();

        return SF.FieldDeclarator(
            fieldKindToken,
            SF.Identifier(identifierToken),
            fieldKind,
            type
        );
    }

    private Operator Operator () {
        Token token = PeekPrevious();
        return token.Kind switch {
            TokenKind.Plus => _Op(OperatorKind.Add),
            TokenKind.Minus => _Op(OperatorKind.Subtract),
            TokenKind.Asterisk => _Op(OperatorKind.Multiply),
            TokenKind.Slash => _Op(OperatorKind.Divide),
            TokenKind.EqualEqual => _Op(OperatorKind.Equals),
            TokenKind.BangEqual => _Op(OperatorKind.NotEquals),
            TokenKind.Less => _Op(OperatorKind.LessThan),
            TokenKind.LessEqual => _Op(OperatorKind.LessThanOrEqualTo),
            TokenKind.Greater => _Op(OperatorKind.GreaterThan),
            TokenKind.GreaterEqual => _Op(OperatorKind.GreaterThanOrEqualTo),
            TokenKind.KwAnd => _Op(OperatorKind.LogicalAnd),
            TokenKind.KwOr => _Op(OperatorKind.LogicalOr),
            _ => throw new Exception(
                $"Cannot parse {token.Kind} as an Operator."
            ),
        };
        Operator _Op (OperatorKind opKind) {
            return SF.Operator(token, opKind);
        }
    }

    private PrivPrintStmt PrivPrintStmt () {
        Token p_printToken = PeekPrevious();
        Expression expr = Expression();

        return SF.PrivPrintStmt(p_printToken, expr);
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