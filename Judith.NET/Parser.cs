using Judith.NET.analysis.syntax;
using Judith.NET.message;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SF = Judith.NET.analysis.syntax.SyntaxFactory;

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

                if (TryConsumeTopLevelNode(out SyntaxNode? node)) {
                    Nodes.Add(node);
                }
                else {
                    Console.WriteLine("TryConsumeTopLevelNode is false!");
                    // TODO: Throw compiler error.
                }
            }
            catch (ParseException) {
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

    /// <summary>
    /// Returns whether the current token matches any of the token kinds given
    /// and, if it does, outputs it to the 'token' parameter.
    /// </summary>
    /// <param name="token">The token matched, if any.</param>
    /// <param name="kind">The kinds ot token to try to match.</param>
    /// <returns></returns>
    private bool TryConsume ([NotNullWhen(true)] out Token? token, params TokenKind[] kinds) {
        foreach (var kind in kinds) {
            if (Check(kind)) {
                token = Advance();
                return true;
            }
        }

        token = null;
        return false;
    }
    #endregion

    #region Parsing methods
    /// <summary>
    /// Returns the next node, whatever it is.
    /// </summary>
    private bool TryConsumeTopLevelNode ([NotNullWhen(true)] out SyntaxNode? node) {
        // TODO: ImportDirective
        // TODO: ModuleDirective
        // TODO: Implementation
        if (TryConsumeHidableItem(out node)) {
            return true;
        }

        if (TryConsumeStatement(out Statement? stmt)) {
            node = stmt;
            return true;
        }

        node = null;
        return false;
    }

    private bool TryConsumeHidableItem ([NotNullWhen(true)] out SyntaxNode? hidable) {
        TryConsume(TokenKind.KwHid, out Token? hidToken);

        // TODO: EnumerateDirective
        // TODO: SymbolDirective

        if (TryConsumeFunctionDefinition(hidToken, out FunctionDefinition? funcDef)) {
            hidable = funcDef;
            return true;
        }
        
        // TODO: TypeDefinition

        hidable = null;
        return false;
    }

    private bool TryConsumeFunctionDefinition (
        Token? hidToken, [NotNullWhen(true)] out FunctionDefinition? funcDef
    ) {
        if (TryConsume(out Token? funcToken, TokenKind.KwFunc, TokenKind.KwGenerator) == false) {
            funcDef = null;
            return false;
        }

        if (TryConsumeIdentifier(out Identifier? identifier) == false) {
            throw Error(CompilerMessage.Parser.IdentifierExpected(Peek().Line));
        }

        if (TryConsumeParameterList(out ParameterList? parameters) == false) {
            throw Error(CompilerMessage.Parser.LeftParenExpected(Peek().Line));
        }

        TryConsumeTypeAnnotation(out TypeAnnotation? returnType);

        if (TryConsumeBlockStatement(null, out BlockStatement? body) == false) {
            throw Error(CompilerMessage.Parser.BodyExpected(Peek().Line));
        }

        funcDef = SF.FunctionDefinition(
            hidToken, funcToken, identifier, parameters, returnType, body
        );
        return true;
    }

    private bool TryConsumeStatement ([NotNullWhen(true)] out Statement? statement) {
        if (TryConsumeLocalDeclarationStatement(out LocalDeclarationStatement? lds)) {
            statement = lds;
            return true;
        }
        if (TryConsumeReturnStatement(out ReturnStatement? returnStmt)) {
            statement = returnStmt;
            return true;
        }
        if (TryConsumeYieldStatement(out YieldStatement? yieldStmt)) {
            statement = yieldStmt;
            return true;
        }
        if (TryConsumeP_PrintStatement(out P_PrintStatement? p_printStmt)) {
            statement = p_printStmt;
            return true;
        }
        // TODO: BreakStatament
        // TODO: ContinueStatament
        if (TryConsumeExpressionStatement(out ExpressionStatement? expr)) {
            statement = expr;
            return true;
        }

        throw Error(CompilerMessage.Parser.UnexpectedToken(Peek().Line, Advance()));
    }

    private bool TryConsumeReturnStatement (
        [NotNullWhen(true)] out ReturnStatement? statement
    ) {
        if (TryConsume(TokenKind.KwReturn, out Token? returnToken) == false) {
            statement = null;
            return false;
        }

        TryConsumeExpression(out Expression? expr);

        statement = SF.ReturnStatement(returnToken, expr);
        return true;
    }

    private bool TryConsumeYieldStatement (
        [NotNullWhen(true)] out YieldStatement? statement
    ) {
        if (TryConsume(TokenKind.KwYield, out Token? yieldToken) == false) {
            statement = null;
            return false;
        }

        if (TryConsumeExpression(out Expression? expr) == false) {
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
        }

        statement = SF.YieldStatement(yieldToken, expr);
        return true;
    }

    private bool TryConsumeLocalDeclarationStatement (
        [NotNullWhen(true)] out LocalDeclarationStatement? statement
    ) {
        if (TryConsume(out Token? mutToken, TokenKind.KwConst, TokenKind.KwVar) == false) {
            statement = null;
            return false;
        }

        LocalKind localKind = mutToken.Kind switch {
            TokenKind.KwConst => LocalKind.Constant,
            TokenKind.KwVar => LocalKind.Variable,
            _ => throw new Exception("TryConsume returned an impossible value.")
        };

        if (TryConsumeLocalDeclaratorList(
            true, localKind, out LocalDeclaratorList? list
        ) == false) {
            throw Error(CompilerMessage.Parser.LocalDeclaratorListExpected(Peek().Line));
        }

        TryConsumeEqualsValueClause(out EqualsValueClause? evc);

        statement = SF.LocalDeclarationStatement(mutToken, list, evc);
        return true;
    }

    private bool TryConsumeBodyStatement (
        TokenKind? openingTokenKind, [NotNullWhen(true)] out BodyStatement? bodyStatement
    ) {
        // Arrow must be tried first, since block statement may not have an
        // opening token (and thus would report an error trying to read an arrow).
        if (TryConsumeArrowStatement(out ArrowStatement? arrowStmt)) {
            bodyStatement = arrowStmt;
            return true;
        }
        if (TryConsumeBlockStatement(openingTokenKind, out BlockStatement? blockStmt)) {
            bodyStatement = blockStmt;
            return true;
        }

        bodyStatement = null;
        return false;
    }

    private bool TryConsumeBlockStatement (
        TokenKind? openingTokenKind, [NotNullWhen(true)] out BlockStatement? blockStatement
    ) {
        Token? openingToken = null;
        if (
            openingTokenKind != null
            && TryConsume(openingTokenKind.Value, out openingToken) == false
        ) {
            blockStatement = null;
            return false;
        }

        List<SyntaxNode> statements = new();
        while (MatchBlockEndingToken() == false && IsAtEnd() == false) {
            if (TryConsumeTopLevelNode(out SyntaxNode? node)) {
                statements.Add(node);
            }
            else {
                throw Error(CompilerMessage.Parser.UnexpectedToken(
                    Peek().Line, Peek()
                ));
            }
        }
        Token closingToken = PeekPrevious();

        blockStatement = SF.BlockStatement(openingToken, statements, closingToken);
        return true;
    }

    private bool TryConsumeArrowStatement (
        [NotNullWhen(true)] out ArrowStatement? arrowStatement
    ) {
        if (TryConsume(TokenKind.EqualArrow, out Token? arrowToken) == false) {
            arrowStatement = null;
            return false;
        }
        if (TryConsumeStatement(out Statement? stmt) == false) {
            throw Error(CompilerMessage.Parser.StatementExpected(Peek().Line));
        }

        arrowStatement = SF.ArrowStatement(arrowToken, stmt);
        return true;
    }

    private bool TryConsumeExpressionStatement (
        [NotNullWhen(true)] out ExpressionStatement? statement
    ) {
        if (TryConsumeExpression(out Expression? expr) == false) {
            statement = null;
            return false;
        }

        statement = SF.ExpressionStatement(expr);
        return true;
    }

    private bool TryConsumeExpression (
        [NotNullWhen(true)] out Expression? expression
    ) {
        if (TryConsumeIfExpression(false, out IfExpression? ifExpr)) {
            expression = ifExpr;
            return true;
        }
        if (TryConsumeMatchExpression(out MatchExpression? matchExpr)) {
            expression = matchExpr;
            return true;
        }
        if (TryConsumeLoopExpression(out LoopExpression? loopExpr)) {
            expression = loopExpr;
            return true;
        }
        if (TryConsumeWhileExpression(out WhileExpression? whileExpr)) {
            expression = whileExpr;
            return true;
        }
        if (TryConsumeForeachExpression(out ForeachExpression? foreachExpr)) {
            expression = foreachExpr;
            return true;
        }
        if (TryConsumeRangeExpression(out Expression? expr)) {
            expression = expr;
            return true;
        }

        expression = null;
        return false;
    }

    private bool TryConsumeIfExpression (
        bool implicitIf, [NotNullWhen(true)] out IfExpression? ifExpression
    ) {
        Token? ifToken = null;
        if (implicitIf == false && TryConsume(TokenKind.KwIf, out ifToken) == false) {
            ifExpression = null;
            return false;
        }
        if (TryConsumeExpression(out Expression? test) == false) {
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
        }
        if (TryConsumeBodyStatement(
            TokenKind.KwThen, out BodyStatement? consequent
        ) == false) {
            throw Error(CompilerMessage.Parser.BodyExpected(Peek().Line));
        }

        if (consequent.Kind == SyntaxKind.BlockStatement) {
            var blockStmt = (BlockStatement)consequent;

            if (blockStmt.ClosingToken == null) throw new Exception(
                "Block consequent needs an closing token."
            );

            if (blockStmt.ClosingToken.Kind == TokenKind.KwElsif) {
                ifExpression = _Elsif();
                return true;
            }
            if (blockStmt.ClosingToken.Kind == TokenKind.KwElse) {
                ifExpression = _Else();
                return true;
            }
            ifExpression = _If();
            return true;
        }
        else if (consequent.Kind == SyntaxKind.ArrowStatement) {
            if (TryConsume(TokenKind.KwElsif, out Token? _)) {
                ifExpression = _Elsif();
                return true;
            }
            if (TryConsume(TokenKind.KwElse, out Token? _)) {
                ifExpression = _Else();
                return true;
            }
            ifExpression = _If();
            return true;
        }
        else {
            throw new Exception("TryConsumeBodyStatement returned an impossible value.");
        }


        IfExpression _Elsif () {
            var elsifToken = PeekPrevious();

            if (TryConsumeIfExpression(true, out IfExpression? alternate) == false) {
                throw Error(CompilerMessage.Parser.ElsifBodyExpected(Peek().Line));
            }

            var alternateStmt = SF.ExpressionStatement(alternate);

            return SF.IfExpression(elsifToken, test, consequent, elsifToken, alternateStmt);
        }

        IfExpression _Else () {
            var elseToken = PeekPrevious();

            if (TryConsumeBodyStatement(null, out BodyStatement? alternate) == false) {
                throw Error(CompilerMessage.Parser.BodyExpected(Peek().Line));
            }

            return SF.IfExpression(elseToken, test, consequent, elseToken, alternate);
        }

        IfExpression _If () {
            if (ifToken == null) {
                throw new Exception("If token not found.");
            }
            return SF.IfExpression(ifToken, test, consequent);
        }
    }

    private bool TryConsumeMatchExpression (
        [NotNullWhen(true)] out MatchExpression? matchExpression
    ) {
        if (TryConsume(TokenKind.KwMatch, out Token? matchToken) == false) {
            matchExpression = null;
            return false;
        }
        if (TryConsumeExpression(out Expression? discriminant) == false) {
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
        }
        if (TryConsume(TokenKind.KwDo ,out Token? doToken) == false) {
            throw Error(CompilerMessage.Parser.DoExpected(Peek().Line));
        }

        List<MatchCase> cases = new();
        while (TryConsumeMatchCase(out MatchCase? matchCase)) {
            cases.Add(matchCase);
        }

        if (TryConsume(TokenKind.KwEnd, out Token? endToken) == false) {
            throw Error(CompilerMessage.Parser.EndExpected(Peek().Line));
        }

        matchExpression = SF.MatchExpression(matchToken, discriminant, doToken, cases, endToken);
        return true;
    }

    private bool TryConsumeLoopExpression (
        [NotNullWhen(true)] out LoopExpression? loopExpression
    ) {
        if (TryConsume(TokenKind.KwLoop, out Token? loopToken) == false) {
            loopExpression = null;
            return false;
        }
        if (TryConsumeBodyStatement(null, out BodyStatement? body) == false) {
            throw Error(CompilerMessage.Parser.BodyExpected(Peek().Line));
        }

        loopExpression = SF.LoopExpression(loopToken, body);
        return true;
    }

    private bool TryConsumeWhileExpression (
        [NotNullWhen(true)] out WhileExpression? whileExpression
    ) {
        if (TryConsume(TokenKind.KwWhile, out Token? whileToken) == false) {
            whileExpression = null;
            return false;
        }
        if (TryConsumeExpression(out Expression? test) == false) {
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
        }
        if (TryConsumeBodyStatement(TokenKind.KwDo, out BodyStatement? body) == false) {
            throw Error(CompilerMessage.Parser.BodyExpected(Peek().Line));
        }

        whileExpression = SF.WhileExpression(whileToken, test, body);
        return true;
    }

    private bool TryConsumeForeachExpression (
        [NotNullWhen(true)] out ForeachExpression? foreachExpression
    ) {
        if (TryConsume(TokenKind.KwFor, out Token? foreachToken) == false) {
            foreachExpression = null;
            return false;
        }

        List<LocalDeclarator> declarators = new();
        LocalKind implicitKind = LocalKind.Constant;
        do {
            if (TryConsumeLocalDeclarator(
                false, implicitKind, out LocalDeclarator? declarator
            ) == false) {
                throw Error(CompilerMessage.Parser.LocalDeclaratorExpected(Peek().Line));
            }

            declarators.Add(declarator);
            implicitKind = declarator.LocalKind;
        } while (Match(TokenKind.Comma));

        if (TryConsume(TokenKind.KwIn, out Token? inToken) == false) {
            throw Error(CompilerMessage.Parser.InExpected(Peek().Line));
        }
        if (TryConsumeExpression(out Expression? enumerable) == false) {
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
        }
        if (TryConsumeBodyStatement(TokenKind.KwDo, out BodyStatement? body) == false) {
            throw Error(CompilerMessage.Parser.BodyExpected(Peek().Line));
        }

        foreachExpression = SF.ForeachExpression(
            foreachToken, declarators, inToken, enumerable, body
        );
        return true;
    }

    private bool TryConsumeRangeExpression (
        [NotNullWhen(true)]  out Expression? expr
    ) {
        // TODO: Implement
        return TryConsumeAssignmentExpression(out expr);
    }

    // "="
    private bool TryConsumeAssignmentExpression (
        [NotNullWhen(true)]  out Expression? expr
    ) {
        if (TryConsumeLogicalExpression(out Expression? leftExpr) == false) {
            return TryConsumeLogicalExpression(out expr);
        }

        if (TryConsumeOperator(out Operator? op, TokenKind.Equal) == false) {
            expr = leftExpr;
            return true;
        }

        if (TryConsumeAssignmentExpression(out Expression? rightExpr) == false) {
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
        }

        expr = SF.AssignmentExpression(leftExpr, op, rightExpr);
        return true;
    }

    // "and" | "or"
    private bool TryConsumeLogicalExpression ([NotNullWhen(true)] out Expression? expr) {
        if (TryConsumeBooleanExpression(out Expression? leftExpr) == false) {
            expr = null;
            return false;
        }

        while (TryConsumeOperator(
            out Operator? op, TokenKind.KwAnd, TokenKind.KwOr
        )) {
            if (TryConsumeBooleanExpression(out Expression? rightExpr) == false) {
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
            }

            leftExpr = SF.BinaryExpression(leftExpr, op, rightExpr);
        }

        expr = leftExpr;
        return true;
    }

    // "==" | "!=" | "~=" | "===" | "!==" | "<" | "<=" | ">" | ">="
    private bool TryConsumeBooleanExpression ([NotNullWhen(true)] out Expression? expr) {
        if (TryConsumeAdditionExpression(out Expression? leftExpr) == false) {
            expr = null;
            return false;
        }

        while (TryConsumeOperator(
            out Operator? op,
            TokenKind.EqualEqual,
            TokenKind.BangEqual,
            TokenKind.TildeEqual,
            TokenKind.EqualEqualEqual,
            TokenKind.BangEqualEqual,
            TokenKind.Less,
            TokenKind.LessEqual,
            TokenKind.Greater,
            TokenKind.GreaterEqual
        )) {
            if (TryConsumeAdditionExpression(out Expression? rightExpr) == false) {
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
            }

            leftExpr = SF.BinaryExpression(leftExpr, op, rightExpr);
        }

        expr = leftExpr;
        return true;
    }

    // "+" | "-"
    private bool TryConsumeAdditionExpression ([NotNullWhen(true)] out Expression? expr) {
        if (TryConsumeMultiplicationExpression(out Expression? leftExpr) == false) {
            expr = null;
            return false;
        }

        while (TryConsumeOperator(
            out Operator? op, TokenKind.Plus, TokenKind.Minus
        )) {
            if (TryConsumeMultiplicationExpression(out Expression? rightExpr) == false) {
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
            }

            leftExpr = SF.BinaryExpression(leftExpr, op, rightExpr);
        }

        expr = leftExpr;
        return true;
    }

    // "*" | "/"
    private bool TryConsumeMultiplicationExpression ([NotNullWhen(true)] out Expression? expr) {
        if (TryConsumeLeftUnaryExpression(out Expression? leftExpr) == false) {
            expr = null;
            return false;
        }

        while (TryConsumeOperator(
            out Operator? op, TokenKind.Asterisk, TokenKind.Slash
        )) {
            if (TryConsumeLeftUnaryExpression(out Expression? rightExpr) == false) {
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
            }

            leftExpr = SF.BinaryExpression(leftExpr, op, rightExpr);
        }

        expr = leftExpr;
        return true;
    }

    // "not" | "-" | "~"
    private bool TryConsumeLeftUnaryExpression ([NotNullWhen(true)] out Expression? expr) {
        if (TryConsumeOperator(
            out Operator? op, TokenKind.KwNot, TokenKind.Minus, TokenKind.Tilde
        )) {
            if (TryConsumeLeftUnaryExpression(out expr) == false) {
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
            }

            expr = SF.LeftUnaryExpression(op, expr);
            return true;
        }

        return TryConsumeMemberAccessExpression(out expr);
    }

    private bool TryConsumeMemberAccessExpression ([NotNullWhen(true)] out Expression? expr) {
        // TODO: Replace with TemplateIdentifierExpression
        if (TryConsumePrimary(out Expression? leftExpr) == false) {
            expr = null;
            return false;
        }

        while (TryConsumeOperator(
            out Operator? op, TokenKind.Dot, TokenKind.DoubleColon
        )) {
            if (TryConsumeIdentifierExpression(out Expression? rightExpr) == false) {
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
            }

            leftExpr = SF.AccessExpression(leftExpr, op, rightExpr);
        }

        expr = leftExpr;
        return true;
    }

    // This always has the highest precedence, and acts as a primary.
    private bool TryConsumeGroupExpression ([NotNullWhen(true)] out Expression? expr) {
        if (TryConsume(TokenKind.LeftParen, out Token? leftParenToken) == false) {
            expr = null;
            return false;
        }

        if (TryConsumeExpression(out expr) == false) {
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
        }

        if (TryConsume(TokenKind.RightParen, out Token? rightParenToken) == false) {
            throw Error(CompilerMessage.Parser.RightParenExpected(Peek().Line));
        }

        expr = SF.GroupExpression(leftParenToken, expr, rightParenToken);
        return true;
    }

    private bool TryConsumePrimary ([NotNullWhen(true)] out Expression? expr) {
        if (TryConsumeGroupExpression(out expr)) return true;
        if (TryConsumeLiteralExpression(out expr)) return true;
        if (TryConsumeIdentifierExpression(out expr)) return true;

        expr = null;
        return false;
    }

    private bool TryConsumeIdentifierExpression (
        [NotNullWhen(true)] out Expression? expr
    ) {
        if (TryConsumeIdentifier(out Identifier? id) == false) {
            expr = null;
            return false;
        }

        expr = SF.IdentifierExpression(id);
        return true;
    }

    private bool TryConsumeLiteralExpression ([NotNullWhen(true)] out Expression? expr) {
        if (TryConsumeLiteral(out Literal? literal) == false) {
            expr = null;
            return false;
        }

        expr = SF.LiteralExpression(literal);
        return true;
    }

    private bool TryConsumeIdentifier ([NotNullWhen(true)] out Identifier? identifier) {
        if (TryConsume(TokenKind.Identifier, out Token? idToken) == false) {
            identifier = null;
            return false;
        }

        identifier = SF.Identifier(idToken);
        return true;
    }

    private bool TryConsumeLiteral ([NotNullWhen(true)] out Literal? literal) {
        if (TryConsume(TokenKind.KwTrue, out Token? token)) {
            literal = MakeBooleanLiteral(token);
            return true;
        }
        if (TryConsume(TokenKind.KwFalse, out token)) {
            literal = MakeBooleanLiteral(token);
            return true;
        }
        if (TryConsume(TokenKind.Number, out token)) {
            literal = MakeNumberLiteral(token);
            return true;
        }
        if (TryConsume(TokenKind.String, out token)) {
            literal = MakeStringLiteral(token);
            return true;
        }

        literal = null;
        return false;
    }

    /// <summary>
    /// If the cursor is at a local declarator list, consumes it.
    /// </summary>
    /// <param name="isFirstImplicit">If true, the first declarator in the list
    /// CANNOT contain "const" or "var".</param>
    /// <param name="impliedLocalKind">The local kind that is implied for the
    /// first declarator in the list.</param>
    /// <param name="list">The list consumed.</param>
    private bool TryConsumeLocalDeclaratorList (
        bool isFirstImplicit,
        LocalKind impliedLocalKind,
        [NotNullWhen(true)] out LocalDeclaratorList? list
    ) {
        LocalDeclaratorKind declaratorKind = LocalDeclaratorKind.Regular;
        if (TryConsume(TokenKind.LeftSquareBracket, out Token? openBracket)) {
            declaratorKind = LocalDeclaratorKind.ArrayPattern;
        }
        else if (TryConsume(TokenKind.LeftCurlyBracket, out openBracket)) {
            declaratorKind |= LocalDeclaratorKind.ObjectPattern;
        }

        if (TryConsumeLocalDeclarator(
            isFirstImplicit, impliedLocalKind, out LocalDeclarator? declarator
        ) == false) {
            list = null;
            return false;
        }

        List<LocalDeclarator> declarators = [declarator];

        while (Match(TokenKind.Comma)) {
            if (TryConsumeLocalDeclarator(
                false, declarator.LocalKind, out declarator
            ) == false) {
                // TODO: This will never happen because TryConsumeLocalDeclarator
                // always fails if it's not parsing a local declarator.
                throw Error(CompilerMessage.Parser.IdentifierExpected(Peek().Line));
            }

            declarators.Add(declarator);
        }

        Token? closeBracket = null;
        if (declaratorKind == LocalDeclaratorKind.ArrayPattern) {
            if (TryConsume(TokenKind.RightSquareBracket, out closeBracket) == false) {
                throw Error(CompilerMessage.Parser.RightSquareBracketExpected(Peek().Line));
            }
        }
        else if (declaratorKind == LocalDeclaratorKind.ObjectPattern) {
            if (TryConsume(TokenKind.RightCurlyBracket, out closeBracket) == false) {
                throw Error(CompilerMessage.Parser.RightCurlyBracketExpected(Peek().Line));
            }
        }

        // Because types work right to left, we can't start assigning types to
        // declarators without types until we have parsed all of them.
        // Note that it is possible for the last declarator not to specify a
        // type, in which case it will propagate "null" as its type.
        // This is valid behavior, as type may be inferred from usage.
        for (int i = declarators.Count - 2; i >= 0; i--) {
            if (declarators[i].TypeAnnotation is null) {
                declarators[i].SetType(declarators[i + 1].TypeAnnotation);
            }
        }

        list = SF.LocalDeclaratorList(openBracket, declaratorKind, declarators, closeBracket);
        return true;
    }

    /// <summary>
    /// If the cursor is at a local declarator, consumes it.
    /// </summary>
    /// <param name="isImplicit">If true, this declarator CANNOT contain "const"
    /// or "var".</param>
    /// <param name="impliedLocalKind">The local kind that is implied if it's not
    /// explicitly defined.</param>
    /// <param name="declarator">The declarator consumed.</param>
    /// <returns></returns>
    private bool TryConsumeLocalDeclarator (
        bool isImplicit,
        LocalKind impliedLocalKind,
        [NotNullWhen(true)] out LocalDeclarator? declarator
    ) {
        if (TryConsume(out Token? mutToken, TokenKind.KwConst, TokenKind.KwVar)) {
            if (isImplicit) {
                throw Error(CompilerMessage.Parser.IdentifierExpected(mutToken.Line));
            }

            impliedLocalKind = mutToken.Kind switch {
                TokenKind.KwConst => LocalKind.Constant,
                TokenKind.KwVar => LocalKind.Variable,
                _ => throw new Exception("TryConsume returned an impossible value.")
            };
        }

        // should this return false and null?
        if (TryConsumeIdentifier(out Identifier? identifier) == false) {
            throw Error(CompilerMessage.Parser.IdentifierExpected(Peek().Line));
        }

        TryConsumeTypeAnnotation(out TypeAnnotation? type);

        declarator = SF.LocalDeclarator(mutToken, identifier, impliedLocalKind, type);
        return true;
    }

    private bool TryConsumeEqualsValueClause (
        [NotNullWhen(true)] out EqualsValueClause? clause
    ) {
        if (TryConsume(TokenKind.Equal, out Token? equalToken) == false) {
            clause = null;
            return false;
        }

        if (TryConsumeExpression(out Expression? expr) == false) {
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
        }

        clause = SF.EqualsValueClause(equalToken, expr);
        return true;
    }

    private bool TryConsumeTypeAnnotation (
        [NotNullWhen(true)] out TypeAnnotation? typeAnnotation
    ) {
        if (TryConsume(TokenKind.Colon, out Token? colonToken) == false) {
            typeAnnotation = null;
            return false;
        }

        if (TryConsumeIdentifier(out Identifier? identifier) == false) {
            throw Error(CompilerMessage.Parser.IdentifierExpected(Peek().Line));
        }

        typeAnnotation = SF.TypeAnnotation(colonToken, identifier);
        return false;
    }

    private bool TryConsumeOperator (
        [NotNullWhen(true)] out Operator? op, params TokenKind[] kinds
    ) {
        if (TryConsume(out Token? opToken, kinds) == false) {
            op = null;
            return false;
        }

        OperatorKind opKind = opToken.Kind switch {
            TokenKind.Plus => OperatorKind.Add,
            TokenKind.Minus => OperatorKind.Subtract,
            TokenKind.Asterisk => OperatorKind.Multiply,
            TokenKind.Slash => OperatorKind.Divide,
            TokenKind.Tilde => OperatorKind.BitwiseNot,
            TokenKind.Equal => OperatorKind.Assignment,
            TokenKind.EqualEqual => OperatorKind.Equals,
            TokenKind.BangEqual => OperatorKind.NotEquals,
            TokenKind.TildeEqual => OperatorKind.Like,
            TokenKind.EqualEqualEqual => OperatorKind.ReferenceEquals,
            TokenKind.BangEqualEqual => OperatorKind.ReferenceNotEquals,
            TokenKind.Less => OperatorKind.LessThan,
            TokenKind.LessEqual => OperatorKind.LessThanOrEqualTo,
            TokenKind.Greater => OperatorKind.GreaterThan,
            TokenKind.GreaterEqual => OperatorKind.GreaterThanOrEqualTo,
            TokenKind.KwAnd => OperatorKind.LogicalAnd,
            TokenKind.KwOr => OperatorKind.LogicalOr,
            TokenKind.Dot => OperatorKind.MemberAccess,
            TokenKind.DoubleColon => OperatorKind.ScopeResolution,
            _ => throw new Exception($"Unknown operator: '{opToken.Kind}'.")
        };

        op = SF.Operator(opToken, opKind);
        return true;
    }

    private bool TryConsumeParameterList (
        [NotNullWhen(true)] out ParameterList? parameterList
    ) {
        if (TryConsume(TokenKind.LeftParen, out Token? leftParenToken) == false) {
            parameterList = null;
            return false;
        }

        List<Parameter> parameters = new();
        if (Check(TokenKind.RightParen) == false) {
            LocalKind impliedKind = LocalKind.Constant;

            do {
                if (TryConsumeLocalDeclarator(
                    false, impliedKind, out LocalDeclarator? declarator
                ) == false) {
                    throw Error(CompilerMessage.Parser.LocalDeclaratorExpected(Peek().Line));
                }

                TryConsumeEqualsValueClause(out EqualsValueClause? defaultValue);

                parameters.Add(SF.Parameter(declarator, defaultValue));
            }
            while (Match(TokenKind.Comma) && Peek().Kind != TokenKind.RightParen);
        }

        if (TryConsume(TokenKind.RightParen, out Token? rightParenToken) == false) {
            throw Error(CompilerMessage.Parser.RightParenExpected(Peek().Line));
        }

        parameterList = SF.ParameterList(leftParenToken, parameters, rightParenToken);
        return true;
    }

    private bool TryConsumeMatchCase ([NotNullWhen(true)] out MatchCase? matchCase) {
        List<Expression> tests = new();

        // If "else" is matched, this is the default case and we don't need to
        // try to get patterns. If it doesn't, then one or more patterns
        // (separated by commas) must appear next.
        if (TryConsume(TokenKind.KwElse, out Token? elseToken) == false) {
            while (TryConsumeLiteralExpression(out Expression? expr)) {
                tests.Add(expr);

                // After each literal, there must be a comma for a subsequent
                // literal to be a valid token.
                if (Match(TokenKind.Comma) == false) {
                    break;
                }
            }
        }

        if (elseToken == null && tests.Count == 0) {
            matchCase = null;
            return false;
        }

        TokenKind? then = elseToken == null ? TokenKind.KwThen : null;
        if (TryConsumeBodyStatement(then, out BodyStatement? consequent) == false) {
            throw Error(CompilerMessage.Parser.BodyExpected(Peek().Line));
        }

        matchCase = SF.MatchCase(elseToken, tests, consequent);
        return true;
    }

    #endregion

    #region Parsing debug statements
    private bool TryConsumeP_PrintStatement (
        [NotNullWhen(true)] out P_PrintStatement? statement
    ) {
        if (TryConsume(TokenKind.PkwPrint, out Token? p_printToken) == false) {
            statement = null;
            return false;
        }
        if (TryConsumeExpression(out Expression? expr) == false) {
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek().Line));
        }

        statement = SF.PrivPrintStmt(p_printToken, expr);
        return true;
    }
    #endregion

    #region Node builders
    private Literal MakeNumberLiteral (Token token) {
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

    private Literal MakeBooleanLiteral (Token token) {
        if (token.Kind == TokenKind.KwTrue) {
            return SF.BooleanLiteral(token, true);
        }
        if (token.Kind == TokenKind.KwFalse) {
            return SF.BooleanLiteral(token, false);
        }

        throw new Exception("Trying to parse an invalid token as a boolean.");
    }

    private Literal MakeStringLiteral (Token token) {
        // TODO: Right now we can only parse strings without flags that start
        // and end with a single delimiter (" or `).
        var delimiter = token.Lexeme[0];
        var str = token.Lexeme[1..^1]
            .Replace("\\\\", "\\")
            .Replace("\\n", "\n")
            .Replace("\\r", "\r");

        return SF.StringLiteral(token, str);
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