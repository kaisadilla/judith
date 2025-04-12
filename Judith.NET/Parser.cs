using Judith.NET.analysis.lexical;
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
    public MessageContainer Messages { get; private set; } = new();
    public bool HasError { get; private set; } = false;

    private readonly List<Token> _tokens;
    private int _cursor = 0;

    public List<SyntaxNode>? Nodes { get; private set; } = null;

    public Parser (List<Token> tokens) {
        _tokens = tokens.Where(t => t.Kind != TokenKind.Comment).ToList(); // TODO: incorporate comments, whitespaces and enters to tokens as trivia in the lexer.
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
        else if (TryConsumeTypedef(hidToken, out TypeDefinition? typedef)) {
            hidable = typedef;
            return true;
        }

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

        if (TryConsumeIdentifier(out SimpleIdentifier? identifier) == false) {
            throw Error(CompilerMessage.Parser.IdentifierExpected(Peek()));
        }

        if (TryConsumeParameterList(out ParameterList? parameters) == false) {
            throw Error(CompilerMessage.Parser.LeftParenExpected(Peek()));
        }

        TryConsumeTypeAnnotation(TokenKind.MinusArrow, out TypeAnnotation? returnType);

        if (TryConsumeBlockBody(null, out BlockBody? body) == false) {
            throw Error(CompilerMessage.Parser.BodyExpected(Peek()));
        }

        funcDef = SF.FunctionDefinition(
            hidToken, funcToken, identifier, parameters, returnType, body
        );
        return true;
    }

    private bool TryConsumeTypedef (
        Token? hidToken, [NotNullWhen(true)] out TypeDefinition? typedef
    ) {
        if (TryConsume(TokenKind.KwTypedef, out Token? typedefToken) == false) {
            typedef = null;
            return false;
        }

        if (TryConsumeStructTypedef(
            hidToken, typedefToken, out StructTypeDefinition? structTypedef
        )) {
            typedef = structTypedef;
            return true;
        }

        throw Error(CompilerMessage.Parser.UnexpectedToken(Peek()));
    }

    private bool TryConsumeStructTypedef (
        Token? hidToken,
        Token typedefToken,
        [NotNullWhen(true)] out StructTypeDefinition? structTypedef
    ) {
        if (TryConsume(TokenKind.KwStruct, out Token? structToken) == false) {
            structTypedef = null;
            return false;
        }

        if (TryConsumeIdentifier(out SimpleIdentifier? id) == false) {
            throw Error(CompilerMessage.Parser.IdentifierExpected(Peek()));
        }

        List<MemberField> memberFields = new();

        while (TryConsumeMemberField(out MemberField? field)) {
            memberFields.Add(field);
        }

        if (TryConsume(TokenKind.KwEnd, out Token? endToken) == false) {
            throw Error(CompilerMessage.Parser.EndExpected(Peek()));
        }

        structTypedef = SF.StructTypeDefinition(
            hidToken, typedefToken, structToken, id, memberFields, endToken
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

        throw Error(CompilerMessage.Parser.UnexpectedToken(Peek()));
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
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
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
            throw Error(CompilerMessage.Parser.LocalDeclaratorListExpected(Peek()));
        }

        TryConsumeEqualsValueClause(out EqualsValueClause? evc);

        statement = SF.LocalDeclarationStatement(mutToken, list, evc);
        return true;
    }

    private bool TryConsumeBody (
        TokenKind? openingTokenKind, [NotNullWhen(true)] out Body? bodyStatement
    ) {
        // Arrow must be tried first, since block statement may not have an
        // opening token (and thus would report an error trying to read an arrow).
        if (TryConsumeArrowBody(out ArrowBody? arrowStmt)) {
            bodyStatement = arrowStmt;
            return true;
        }
        if (TryConsumeBlockBody(openingTokenKind, out BlockBody? blockStmt)) {
            bodyStatement = blockStmt;
            return true;
        }

        bodyStatement = null;
        return false;
    }

    private bool TryConsumeBlockBody (
        TokenKind? openingTokenKind, [NotNullWhen(true)] out BlockBody? blockStatement
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
                throw Error(CompilerMessage.Parser.UnexpectedToken(Peek()));
            }
        }
        Token closingToken = PeekPrevious();

        blockStatement = SF.BlockBody(openingToken, statements, closingToken);
        return true;
    }

    private bool TryConsumeArrowBody (
        [NotNullWhen(true)] out ArrowBody? arrowBody
    ) {
        if (TryConsume(TokenKind.EqualArrow, out Token? arrowToken) == false) {
            arrowBody = null;
            return false;
        }
        if (TryConsumeExpression(out Expression? expr) == false) {
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
        }

        arrowBody = SF.ArrowBody(arrowToken, expr);
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
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
        }
        if (TryConsumeBody(
            TokenKind.KwThen, out Body? consequent
        ) == false) {
            throw Error(CompilerMessage.Parser.BodyExpected(Peek()));
        }

        if (consequent.Kind == SyntaxKind.BlockBody) {
            var blockStmt = (BlockBody)consequent;

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
        else if (consequent.Kind == SyntaxKind.ArrowBody) {
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
                throw Error(CompilerMessage.Parser.ElsifBodyExpected(Peek()));
            }

            var alternateBlock = SF.ExpressionBody(alternate);

            return SF.IfExpression(elsifToken, test, consequent, elsifToken, alternateBlock);
        }

        IfExpression _Else () {
            var elseToken = PeekPrevious();

            if (TryConsumeBody(null, out Body? alternate) == false) {
                throw Error(CompilerMessage.Parser.BodyExpected(Peek()));
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
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
        }
        if (TryConsume(TokenKind.KwDo ,out Token? doToken) == false) {
            throw Error(CompilerMessage.Parser.DoExpected(Peek()));
        }

        List<MatchCase> cases = new();
        while (TryConsumeMatchCase(out MatchCase? matchCase)) {
            cases.Add(matchCase);
        }

        if (TryConsume(TokenKind.KwEnd, out Token? endToken) == false) {
            throw Error(CompilerMessage.Parser.EndExpected(Peek()));
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
        if (TryConsumeBody(null, out Body? body) == false) {
            throw Error(CompilerMessage.Parser.BodyExpected(Peek()));
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
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
        }
        if (TryConsumeBody(TokenKind.KwDo, out Body? body) == false) {
            throw Error(CompilerMessage.Parser.BodyExpected(Peek()));
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
                throw Error(CompilerMessage.Parser.LocalDeclaratorExpected(Peek()));
            }

            declarators.Add(declarator);
            implicitKind = declarator.LocalKind;
        } while (Match(TokenKind.Comma));

        if (TryConsume(TokenKind.KwIn, out Token? inToken) == false) {
            throw Error(CompilerMessage.Parser.InExpected(Peek()));
        }
        if (TryConsumeExpression(out Expression? enumerable) == false) {
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
        }
        if (TryConsumeBody(TokenKind.KwDo, out Body? body) == false) {
            throw Error(CompilerMessage.Parser.BodyExpected(Peek()));
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
        if (TryConsumeLogicalAndExpression(out Expression? leftExpr) == false) {
            expr = null;
            return false;
        }

        if (TryConsumeOperator(out Operator? op, TokenKind.Equal) == false) {
            expr = leftExpr;
            return true;
        }

        if (TryConsumeAssignmentExpression(out Expression? rightExpr) == false) {
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
        }

        expr = SF.AssignmentExpression(leftExpr, op, rightExpr);
        return true;
    }

    // "and"
    private bool TryConsumeLogicalAndExpression ([NotNullWhen(true)] out Expression? expr) {
        if (TryConsumeLogicalOrExpression(out Expression? leftExpr) == false) {
            expr = null;
            return false;
        }

        while (TryConsumeOperator(out Operator? op, TokenKind.KwAnd)) {
            if (TryConsumeLogicalOrExpression(out Expression? rightExpr) == false) {
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
            }

            leftExpr = SF.BinaryExpression(leftExpr, op, rightExpr);
        }

        expr = leftExpr;
        return true;
    }

    // "or"
    private bool TryConsumeLogicalOrExpression ([NotNullWhen(true)] out Expression? expr) {
        if (TryConsumeBooleanExpression(out Expression? leftExpr) == false) {
            expr = null;
            return false;
        }

        while (TryConsumeOperator(out Operator? op, TokenKind.KwOr)) {
            if (TryConsumeBooleanExpression(out Expression? rightExpr) == false) {
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
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
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
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
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
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
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
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
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
            }

            expr = SF.LeftUnaryExpression(op, expr);
            return true;
        }

        return TryConsumeObjectInitializationExpression(out expr);
    }

    // primary_expr? obj_initializer
    private bool TryConsumeObjectInitializationExpression (
        [NotNullWhen(true)] out Expression? expr
    ) {
        // Because provider can be implicit (e.g. "{ num = 3, num2 = 5 }"), we
        // can't discard an object initialization expresssion just yet.
        TryConsumeCallExpression(out Expression? provider);

        // If there's no initializer block, then whether this parse failed or
        // succeeded depends on whether we found something as a provider.
        if (TryConsumeObjectInitializer(out ObjectInitializer? objInit) == false) {
            expr = provider;
            return expr != null;
        }

        expr = SF.ObjectInitializationExpression(provider, objInit);
        return true;
    }

    // "(" arglist? ")"
    private bool TryConsumeCallExpression ([NotNullWhen(true)] out Expression? expr) {
        if (TryConsumeAccessExpression(out Expression? leftExpr) == false) {
            expr = null;
            return false;
        }

        if (TryConsumeArgumentList(out ArgumentList? argList) == false) {
            expr = leftExpr;
            return true;
        }

        expr = SF.CallExpression(leftExpr, argList);
        return true;
    }

    private bool TryConsumeAccessExpression ([NotNullWhen(true)] out Expression? expr) {
        // Because member access can be implicit (e.g. ".name"), we
        // can't discard a member access just because we didn't find what it's
        // accessing.
        TryConsumePrimary(out Expression? receiver);

        // Now, if we don't find a member access token:
        if (Peek().Kind != TokenKind.Dot) {
            // IF we didn't find a provider, then we aren't parsing an expression
            // that stems from this one:
            if (receiver == null) {
                expr = null;
                return false;
            }
            // If we did, then that provider becomes the expression.
            else {
                expr = receiver;
                return true;
            }
        }

        while (TryConsumeOperator(out Operator? op, TokenKind.Dot)) {
            if (TryConsumeIdentifier(out SimpleIdentifier? member) == false) {
                throw Error(CompilerMessage.Parser.IdentifierExpected(Peek()));
            }

            receiver = SF.AccessExpression(receiver, op, member);
        }

        if (receiver == null) throw new("Receive cannot be null here.");

        expr = receiver;
        return true;
    }

    private bool TryConsumePrimary ([NotNullWhen(true)] out Expression? expr) {
        if (TryConsumeGroupExpression(out expr)) return true;
        if (TryConsumeLiteralExpression(out expr)) return true;
        if (TryConsumeIdentifierExpression(out expr)) return true;

        expr = null;
        return false;
    }

    // This always has the highest precedence, and acts as a primary.
    private bool TryConsumeGroupExpression ([NotNullWhen(true)] out Expression? expr) {
        if (TryConsume(TokenKind.LeftParen, out Token? leftParenToken) == false) {
            expr = null;
            return false;
        }

        if (TryConsumeExpression(out expr) == false) {
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
        }

        if (TryConsume(TokenKind.RightParen, out Token? rightParenToken) == false) {
            throw Error(CompilerMessage.Parser.RightParenExpected(Peek()));
        }

        expr = SF.GroupExpression(leftParenToken, expr, rightParenToken);
        return true;
    }

    private bool TryConsumeIdentifierExpression (
        [NotNullWhen(true)] out Expression? expr
    ) {
        if (TryConsumeIdentifier(out SimpleIdentifier? id) == false) {
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

    private bool TryConsumeQualifiedIdentifier (
        [NotNullWhen(true)] out Identifier? identifier
    ) {
        if (TryConsumeIdentifier(out SimpleIdentifier? qualifier) == false) {
            identifier = null;
            return false;
        }

        identifier = qualifier;

        while (TryConsumeOperator(out Operator? op, TokenKind.DoubleColon)) {
            if (TryConsumeIdentifier(out SimpleIdentifier? name) == false) {
                throw Error(CompilerMessage.Parser.IdentifierExpected(Peek()));
            }

            identifier = SF.QualifiedIdentifier(identifier, op, name);
        }

        return true;
    }

    private bool TryConsumeIdentifier ([NotNullWhen(true)] out SimpleIdentifier? identifier) {
        if (TryConsume(TokenKind.Identifier, out Token? idToken) == false) {
            identifier = null;
            return false;
        }

        identifier = SF.SimpleIdentifier(idToken);
        return true;
    }

    private bool TryConsumeLiteral ([NotNullWhen(true)] out Literal? literal) {
        if (TryConsume(TokenKind.KwTrue, out Token? token)) {
            literal = SF.Literal(token);
            return true;
        }
        if (TryConsume(TokenKind.KwFalse, out token)) {
            literal = SF.Literal(token);
            return true;
        }
        if (TryConsume(TokenKind.Number, out token)) {
            literal = SF.Literal(token);
            return true;
        }
        if (TryConsume(TokenKind.String, out token)) {
            literal = SF.Literal(token);
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
                throw Error(CompilerMessage.Parser.IdentifierExpected(Peek()));
            }

            declarators.Add(declarator);
        }

        Token? closeBracket = null;
        if (declaratorKind == LocalDeclaratorKind.ArrayPattern) {
            if (TryConsume(TokenKind.RightSquareBracket, out closeBracket) == false) {
                throw Error(CompilerMessage.Parser.RightSquareBracketExpected(Peek()));
            }
        }
        else if (declaratorKind == LocalDeclaratorKind.ObjectPattern) {
            if (TryConsume(TokenKind.RightCurlyBracket, out closeBracket) == false) {
                throw Error(CompilerMessage.Parser.RightCurlyBracketExpected(Peek()));
            }
        }

        // Because types work right to left, we can't start assigning types to
        // declarators without types until we have parsed all of them.
        // Note that it is possible for the last declarator not to specify a
        // type, in which case it will propagate "null" as its type.
        // This is valid behavior, as type may be inferred from usage.
        for (int i = declarators.Count - 2; i >= 0; i--) {
            if (declarators[i].TypeAnnotation is null) {
                declarators[i].SetTypeAnnotation(declarators[i + 1].TypeAnnotation);
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
                throw Error(CompilerMessage.Parser.IdentifierExpected(mutToken));
            }

            impliedLocalKind = mutToken.Kind switch {
                TokenKind.KwConst => LocalKind.Constant,
                TokenKind.KwVar => LocalKind.Variable,
                _ => throw new Exception("TryConsume returned an impossible value.")
            };
        }

        // should this return false and null?
        if (TryConsumeIdentifier(out SimpleIdentifier? identifier) == false) {
            throw Error(CompilerMessage.Parser.IdentifierExpected(Peek()));
        }

        TryConsumeTypeAnnotation(TokenKind.Colon, out TypeAnnotation ? type);

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

        List<Expression> expressions = [];

        do {
            if (TryConsumeExpression(out Expression? expr) == false) {
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
            }

            expressions.Add(expr);
        } while (Match(TokenKind.Comma));

        clause = SF.EqualsValueClause(equalToken, expressions);
        return true;
    }

    private bool TryConsumeTypeAnnotation (
        TokenKind delimiter,
        [NotNullWhen(true)] out TypeAnnotation? typeAnnotation
    ) {
        if (TryConsume(delimiter, out Token? delimiterToken) == false) {
            typeAnnotation = null;
            return false;
        }

        if (TryConsumeType(out TypeNode? type) == false) {
            throw Error(CompilerMessage.Parser.TypeExpected(Peek()));
        }

        typeAnnotation = SF.TypeAnnotation(delimiterToken, type);
        return true;
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
                    throw Error(CompilerMessage.Parser.LocalDeclaratorExpected(Peek()));
                }

                TryConsumeEqualsValueClause(out EqualsValueClause? defaultValue);

                parameters.Add(SF.Parameter(declarator, defaultValue));
            }
            while (Match(TokenKind.Comma) && Peek().Kind != TokenKind.RightParen);
        }

        if (TryConsume(TokenKind.RightParen, out Token? rightParenToken) == false) {
            throw Error(CompilerMessage.Parser.RightParenExpected(Peek()));
        }

        // If the function has parameters.
        if (parameters.Count > 0) {
            // The last parameter must have a type annotation. Types are not
            // inferred in parameters, even if they have default values.
            if (parameters[^1].Declarator.TypeAnnotation == null) {
                throw Error(CompilerMessage.Parser.ParameterTypeMustBeSpecified(Peek()));
            }

            // Each parameter, from right to left, inherits the type annotation
            // of the one to its right if it doesn't have one.
            TypeAnnotation inherited = parameters[^1].Declarator.TypeAnnotation!;

            for (int i = parameters.Count - 2; i >= 0; i--) {
                if (parameters[i].Declarator.TypeAnnotation == null) {
                    parameters[i].Declarator.SetTypeAnnotation(inherited);
                }
                else {
                    inherited = parameters[i].Declarator.TypeAnnotation!;
                }
            }
        }

        parameterList = SF.ParameterList(leftParenToken, parameters, rightParenToken);
        return true;
    }

    private bool TryConsumeArgumentList ([NotNullWhen(true)] out ArgumentList? argList) {
        if (TryConsume(TokenKind.LeftParen, out Token? leftParenToken) == false) {
            argList = null;
            return false;
        }

        List<Argument> arguments = new();
        if (Check(TokenKind.RightParen) == false) {
            do {
                if (TryConsumeArgument(out Argument? argument) == false) {
                    throw Error(CompilerMessage.Parser.ArgumentExpected(Peek()));
                }

                arguments.Add(argument);
            }
            while (Match(TokenKind.Comma) && Peek().Kind != TokenKind.RightParen); // TODO: Here it should be Check(TokenKind.RightParen) ???
        }

        if (TryConsume(TokenKind.RightParen, out Token? rightParenToken) == false) {
            throw Error(CompilerMessage.Parser.RightParenExpected(Peek()));
        }

        argList = SF.ArgumentList(leftParenToken, arguments, rightParenToken);
        return true;
    }

    private bool TryConsumeArgument ([NotNullWhen(true)] out Argument? argument) {
        if (TryConsumeExpression(out Expression? expr) == false) {
            argument = null;
            return false;
        }

        argument = SF.Argument(expr);
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
        if (TryConsumeBody(then, out Body? consequent) == false) {
            throw Error(CompilerMessage.Parser.BodyExpected(Peek()));
        }

        matchCase = SF.MatchCase(elseToken, tests, consequent);
        return true;
    }

    private bool TryConsumeObjectInitializer (
        [NotNullWhen(true)] out ObjectInitializer? objInit
    ) {
        if (TryConsume(TokenKind.LeftCurlyBracket, out Token? leftBracket) == false) {
            objInit = null;
            return false;
        }

        List<FieldInitialization> fieldInits = new();

        do {
            if (TryConsumeFieldInitialization(
                out FieldInitialization? fieldInit
            ) == false) throw Error(
                CompilerMessage.Parser.FieldInitializationExpected(Peek())
            );

            fieldInits.Add(fieldInit);
        }
        while (Match(TokenKind.Comma) && Peek().Kind != TokenKind.RightCurlyBracket);

        if (TryConsume(TokenKind.RightCurlyBracket, out Token? rightBracket) == false) {
            throw Error(CompilerMessage.Parser.RightCurlyBracketExpected(Peek()));
        }

        objInit = SF.ObjectInitializer(leftBracket, fieldInits, rightBracket);
        return true;
    }

    private bool TryConsumeFieldInitialization (
        [NotNullWhen(true)] out FieldInitialization? init
    ) {
        if (TryConsumeIdentifier(out SimpleIdentifier? id) == false) {
            init = null;
            return false;
        }

        if (TryConsumeEqualsValueClause(out EqualsValueClause? clause) == false) {
            throw Error(CompilerMessage.Parser.FieldMustBeInitialized(Peek()));
        }

        init = SF.FieldInitialization(id, clause);
        return true;
    }

    private bool TryConsumeMemberField ([NotNullWhen(true)] out MemberField? field) {
        TryConsume(out Token? accessToken, TokenKind.KwPub, TokenKind.KwHid);
        TryConsume(TokenKind.KwStatic, out Token? staticToken);
        TryConsume(TokenKind.KwMut, out Token? mutToken);
        TryConsume(TokenKind.KwConst, out Token? constToken);

        if (TryConsumeIdentifier(out SimpleIdentifier? id) == false) {
            field = null;
            return false;
        }

        if (TryConsumeTypeAnnotation(TokenKind.Colon, out TypeAnnotation ? typeAnnotation) == false) {
            throw Error(CompilerMessage.Parser.TypeAnnotationExpected(Peek()));
        }

        TryConsumeEqualsValueClause(out EqualsValueClause? evc);

        field = SF.MemberField(
            accessToken, staticToken, mutToken, constToken, id, typeAnnotation, evc
        );

        return true;
    }

    private bool TryConsumeType ([NotNullWhen(true)] out TypeNode? type) {
        return TryConsumeUnionType(out type);
    }

    private bool TryConsumeUnionType ([NotNullWhen(true)] out TypeNode? type) {
        if (TryConsumeRawArrayType(out type) == false) {
            return false;
        }

        if (Peek().Kind != TokenKind.Pipe) {
            return true;
        }

        List<TypeNode> memberTypes = [type];
        while (Match(TokenKind.Pipe)) {
            if (TryConsumeRawArrayType(out type) == false) {
                throw Error(CompilerMessage.Parser.TypeExpected(Peek()));
            }

            memberTypes.Add(type);
        }

        type = SF.UnionType(memberTypes);
        return true;
    }

    private bool TryConsumeRawArrayType ([NotNullWhen(true)] out TypeNode? type) {
        TryConsume(TokenKind.KwConst, out Token? constToken);

        if (TryConsumePrimaryType(out type) == false) {
            if (constToken == null) return false;
            else throw Error(CompilerMessage.Parser.TypeExpected(Peek()));
        }

        if (Peek().Kind != TokenKind.LeftSquareBracket) {
            type = SF.SetTypeConstness(type, constToken);
            return true;
        }

        while (TryConsume(TokenKind.LeftSquareBracket, out Token? leftToken)) {
            if (TryConsumeExpression(out Expression? length) == false) {
                throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
            }
            if (TryConsume(TokenKind.RightSquareBracket, out Token? rightToken) == false) {
                throw Error(CompilerMessage.Parser.RightSquareBracketExpected(Peek()));
            }

            type = SF.RawArrayType(type, leftToken, length, rightToken);

            if (TryConsume(TokenKind.QuestionMark, out Token? questionToken)) {
                type = SF.SetNullability(type, questionToken);
            }
        }

        if (constToken != null) {
            type = SF.SetTypeConstness(type, constToken);
        }

        return true;
    }

    private bool TryConsumePrimaryType ([NotNullWhen(true)] out TypeNode? type) {
        bool typeFound = TryConsumeFunctionType(out type)
            || TryConsumeTupleArrayType(out type)
            //|| TryConsumeObjectType(out type) // TODO
            || TryConsumeLiteralType(out type)
            || TryConsumeIdentifierType(out type);

        if (typeFound == false || type == null) return false;

        if (TryConsume(TokenKind.QuestionMark, out Token? questionToken)) {
            type = SF.SetNullability(type, questionToken);
        }

        return true;
    }

    private bool TryConsumeFunctionType ([NotNullWhen(true)] out TypeNode? type) {
        if (TryConsume(TokenKind.LeftParen, out Token? leftParen) == false) {
            type = null;
            return false;
        }

        List<TypeNode> paramTypes = [];

        // If the parenthesis is not immediately closed.
        if (Check(TokenKind.RightParen)) {
            do {
                if (TryConsumeType(out TypeNode? paramType) == false) {
                    throw Error(CompilerMessage.Parser.TypeExpected(Peek()));
                }

                paramTypes.Add(paramType);
            }
            while (Match(TokenKind.Comma) && Peek().Kind != TokenKind.RightParen);
        }

        if (TryConsume(TokenKind.RightParen, out Token? rightParen) == false) {
            throw Error(CompilerMessage.Parser.RightParenExpected(Peek()));
        }

        // If we don't find '->', then it's a group type.
        if (TryConsume(TokenKind.MinusArrow, out Token? arrow) == false) {
            // A single type between parenthesis becomes a group type.
            if (paramTypes.Count == 1) {
                type = SF.GroupType(leftParen, paramTypes[0], rightParen);
                return true;
            }
            // This is an error because a type was expected inside the parenthesis.
            else if (paramTypes.Count == 0) {
                throw Error(CompilerMessage.Parser.TypeExpected(rightParen));
            }
            // This is an error because a ')' was expected after the first type.
            else {
                throw Error(CompilerMessage.Parser.RightParenExpected(paramTypes[1]));
            }
        }

        if (TryConsumeType(out TypeNode? returnType) == false) {
            throw Error(CompilerMessage.Parser.TypeExpected(Peek()));
        }

        type = SF.FunctionType(leftParen, paramTypes, rightParen, returnType);
        return true;
    }

    private bool TryConsumeTupleArrayType ([NotNullWhen(true)] out TypeNode? type) {
        if (TryConsume(TokenKind.LeftSquareBracket, out Token? leftSqBracket) == false) {
            type = null;
            return false;
        }

        if (Check(TokenKind.RightSquareBracket)) {
            throw Error(CompilerMessage.Parser.TypeExpected(Peek()));
        }

        List<TypeNode> memberTypes = [];
        do {
            if (TryConsumeType(out TypeNode? paramType) == false) {
                throw Error(CompilerMessage.Parser.TypeExpected(Peek()));
            }

            memberTypes.Add(paramType);
        }
        while (Match(TokenKind.Comma) && Peek().Kind != TokenKind.RightParen);

        if (TryConsume(TokenKind.RightSquareBracket, out Token? rightSqBracket) == false) {
            throw Error(CompilerMessage.Parser.RightSquareBracketExpected(Peek()));
        }

        type = SF.TupleArrayType(leftSqBracket, memberTypes, rightSqBracket);
        return true;
    }

    private bool TryConsumeLiteralType ([NotNullWhen(true)] out TypeNode? type) {
        if (TryConsumeLiteral(out Literal? lit) == false) {
            type = null;
            return false;
        }

        type = SF.LiteralType(lit);
        return true;
    }

    private bool TryConsumeIdentifierType ([NotNullWhen(true)] out TypeNode? type) {
        if (TryConsumeQualifiedIdentifier(out Identifier? qid) == false) {
            type = null;
            return false;
        }

        type = SF.IdentifierType(qid);
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
            throw Error(CompilerMessage.Parser.ExpressionExpected(Peek()));
        }

        statement = SF.PrivPrintStmt(p_printToken, expr);
        return true;
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
        Messages.Add(error);

        return new(error);
    }
}

public class ParseException : Exception {
    public CompilerMessage CompilerMessage { get; private set; }

    public ParseException (CompilerMessage message) : base(message.Message) {
        CompilerMessage = message;
    }
}
