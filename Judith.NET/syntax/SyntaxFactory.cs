using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;
public static class SyntaxFactory {
    public static FunctionDefinition FunctionDefinition (
        Token? hidToken,
        Token funcToken,
        Identifier name,
        ParameterList parameters,
        TypeAnnotation? returnType,
        Statement body
    ) {
        var funcItem = new FunctionDefinition(
            false, hidToken != null, name, parameters, returnType, body
        ) {
            HidToken = hidToken,
            FuncToken = funcToken,
        };
        funcItem.SetSpan(new(funcToken.Start, body.Span.End));
        funcItem.SetLine(funcToken.Line);

        return funcItem;
    }

    public static BlockStatement BlockStatement (
        Token? openingToken, List<SyntaxNode> statements, Token closingToken
    ) {
        var blockStmt = new BlockStatement(statements) {
            OpeningToken = openingToken,
            ClosingToken = closingToken,
        };

        if (openingToken != null) {
            blockStmt.SetSpan(new(openingToken.Start, closingToken.End));
            blockStmt.SetLine(openingToken.Line);
        }
        else if (statements.Count > 0) {
            blockStmt.SetSpan(new(statements[0].Span.Start, closingToken.End));
            blockStmt.SetLine(statements[0].Line);
        }
        else {
            blockStmt.SetSpan(new(closingToken.Start, closingToken.End));
            blockStmt.SetLine(closingToken.Line);
        }

        return blockStmt;
    }

    public static ArrowStatement ArrowStatement (
        Token arrowToken, Statement statement
    ) {
        var arrowStmt = new ArrowStatement(statement) {
            ArrowToken = arrowToken,
        };
        arrowStmt.SetSpan(new(arrowToken.Start, statement.Span.End));
        arrowStmt.SetLine(arrowToken.Line);

        return arrowStmt;
    }

    public static LocalDeclarationStatement LocalDeclarationStatement (
        Token declarationToken,
        LocalDeclaratorList declaratorList,
        EqualsValueClause? initializer
    ) {
        var localDeclStmt = new LocalDeclarationStatement(declaratorList, initializer) {
            DeclaratorToken = declarationToken,
        };

        int end = initializer?.Span.End ?? declaratorList.Span.End;
        localDeclStmt.SetSpan(new(declarationToken.Start, end));
        localDeclStmt.SetLine(declarationToken.Line);

        return localDeclStmt;
    }

    public static ReturnStatement ReturnStatement (
        Token returnToken, Expression? expression
    ) {
        var returnStmt = new ReturnStatement(expression) {
            ReturnToken = returnToken,
        };
        returnStmt.SetSpan(new(returnToken.Start, expression?.Span.End ?? returnToken.End));
        returnStmt.SetLine(returnToken.Line);

        return returnStmt;
    }

    public static YieldStatement YieldStatement (
        Token yieldToken, Expression expression
    ) {
        var yieldStmt = new YieldStatement(expression) {
            YieldToken = yieldToken,
        };
        yieldStmt.SetSpan(new(yieldToken.Start, expression.Span.End));
        yieldStmt.SetLine(yieldToken.Line);

        return yieldStmt;
    }

    public static ExpressionStatement ExpressionStatement (Expression expr) {
        var exprStmt = new ExpressionStatement(expr);
        exprStmt.SetSpan(expr.Span);
        exprStmt.SetLine(expr.Line);

        return exprStmt;
    }

    public static IfExpression IfExpression (
        Token ifToken, Expression test, Statement consequent
    ) {
        var ifExpr = new IfExpression(test, consequent, null) {
            IfToken = ifToken,
            ElseToken = null,
        };
        ifExpr.SetSpan(new(ifToken.Start, consequent.Span.End));
        ifExpr.SetLine(ifExpr.IfToken.Line);

        return ifExpr;
    }

    public static IfExpression IfExpression (
        Token ifToken,
        Expression test,
        Statement consequent,
        Token elseToken,
        Statement alternate
    ) {
        var ifExpr = new IfExpression(test, consequent, alternate) {
            IfToken = ifToken,
            ElseToken = elseToken,
        };
        ifExpr.SetSpan(new(ifToken.Start, alternate.Span.End));
        ifExpr.SetLine(ifExpr.IfToken.Line);

        return ifExpr;
    }

    public static MatchExpression MatchExpression (
        Token matchToken,
        Expression discriminant,
        Token doToken,
        List<MatchCase> cases,
        Token endToken
    ) {
        var matchExpr = new MatchExpression(discriminant, cases) {
            MatchToken = matchToken,
            DoToken = doToken,
            EndToken = endToken,
        };
        matchExpr.SetSpan(new(matchToken.Start, endToken.End));
        matchExpr.SetLine(matchExpr.MatchToken.Line);

        return matchExpr;
    }

    public static LoopExpression LoopExpression (Token loopToken, Statement body) {
        var loopExpr = new LoopExpression(body) {
            LoopToken = loopToken,
        };
        loopExpr.SetSpan(new(loopToken.Start, body.Span.End));
        loopExpr.SetLine(loopToken.Line);

        return loopExpr;
    }

    public static WhileExpression WhileExpression (
        Token whileToken, Expression test, Statement body
    ) {
        var whileExpr = new WhileExpression(test, body) {
            WhileToken = whileToken,
        };
        whileExpr.SetSpan(new(whileToken.Start, body.Span.End));
        whileExpr.SetLine(whileToken.Line);

        return whileExpr;
    }

    public static ForeachExpression ForeachExpression (
        Token foreachToken,
        List<LocalDeclarator> declarators,
        Token inToken,
        Expression enumerable,
        Statement body
    ) {
        var foreachExpr = new ForeachExpression(declarators, enumerable, body) {
            ForeachToken = foreachToken,
            InToken = inToken,
        };
        foreachExpr.SetSpan(new(foreachToken.Start, body.Span.End));
        foreachExpr.SetLine(foreachToken.Line);

        return foreachExpr;
    }

    public static AssignmentExpression AssignmentExpression (
        Expression left, Operator op, Expression right
    ) {
        var assignExpr = new AssignmentExpression(left, op, right);
        assignExpr.SetSpan(new(assignExpr.Left.Span.Start, assignExpr.Right.Span.End));
        assignExpr.SetLine(assignExpr.Left.Line);

        return assignExpr;
    }

    public static BinaryExpression BinaryExpression (
        Expression left, Operator op, Expression right
    ) {
        var binaryExpr = new BinaryExpression(op, left, right);
        binaryExpr.SetSpan(new(left.Span.Start, right.Span.End));
        binaryExpr.SetLine(binaryExpr.Left.Line);

        return binaryExpr;
    }

    public static LeftUnaryExpression LeftUnaryExpression (
        Operator op, Expression expr
    ) {
        var unaryExpr = new LeftUnaryExpression(op, expr);
        unaryExpr.SetSpan(new(op.Span.Start, expr.Span.End));
        unaryExpr.SetLine(unaryExpr.Operator.Line);

        return unaryExpr;
    }

    public static AccessExpression AccessExpression (
        Expression leftExpr, Operator op, Expression rightExpr
    ) {
        var accessExpr = new AccessExpression(leftExpr, op, rightExpr);
        accessExpr.SetSpan(new(leftExpr.Span.Start, rightExpr.Span.End));
        accessExpr.SetLine(leftExpr.Line);

        return accessExpr;
    }

    public static GroupExpression GroupExpression (
        Token leftParen, Expression expr, Token rightParen
    ) {
        var groupExpr = new GroupExpression(expr) {
            LeftParenthesisToken = leftParen,
            RightParenthesisToken = rightParen,
        };
        groupExpr.SetSpan(new(leftParen.Start, rightParen.End));
        groupExpr.SetLine(groupExpr.LeftParenthesisToken.Line);

        return groupExpr;
    }

    public static IdentifierExpression IdentifierExpression (Identifier identifier) {
        var idExpr = new IdentifierExpression(identifier);
        idExpr.SetSpan(identifier.Span);
        idExpr.SetLine(identifier.Line);

        return idExpr;
    }

    public static LiteralExpression LiteralExpression (Literal literal) {
        var litExpr = new LiteralExpression(literal);
        litExpr.SetSpan(literal.Span);
        litExpr.SetLine(literal.Line);

        return litExpr;
    }

    public static Identifier Identifier (Token rawToken) {
        var id = new Identifier(rawToken);
        id.SetSpan(new(rawToken.Start, rawToken.End));
        id.SetLine(rawToken.Line);

        return id;
    }

    public static Literal Literal (Token rawToken) {
        var lit = new Literal(LiteralKind.Unknown) {
            RawToken = rawToken,
        };
        lit.SetSpan(new(rawToken.Start, rawToken.End));
        lit.SetLine(lit.RawToken.Line);

        return lit;
    }

    public static Literal NumberLiteral (Token rawToken, double value) {
        var lit = new Literal(LiteralKind.Float64) {
            RawToken = rawToken,
        };
        lit.SetSpan(new(rawToken.Start, rawToken.End));
        lit.SetLine(lit.RawToken.Line);
        lit.SetValue(value);

        return lit;
    }

    public static Literal NumberLiteral (Token rawToken, long value) {
        var lit = new Literal(LiteralKind.Int64) {
            RawToken = rawToken,
        };
        lit.SetSpan(new(rawToken.Start, rawToken.End));
        lit.SetLine(lit.RawToken.Line);
        lit.SetValue(value);

        return lit;
    }

    public static Literal BooleanLiteral (Token rawToken, bool value) {
        var lit = new Literal(LiteralKind.Keyword) {
            RawToken = rawToken,
        };
        lit.SetSpan(new(rawToken.Start, rawToken.End));
        lit.SetLine(lit.RawToken.Line);
        lit.SetValue(value);

        return lit;
    }

    public static Literal StringLiteral (Token rawToken, string value) {
        var lit = new Literal(LiteralKind.String) {
            RawToken = rawToken,
        };
        lit.SetSpan(new(rawToken.Start, rawToken.End));
        lit.SetLine(lit.RawToken.Line);
        lit.SetValue(value);

        return lit;
    }

    public static LocalDeclaratorList LocalDeclaratorList (
        Token? declaratorKindOpeningToken,
        LocalDeclaratorKind declaratorKind,
        List<LocalDeclarator> declarators,
        Token? declaratorKindClosingToken
    ) {
        var localDeclaratorList = new LocalDeclaratorList(declaratorKind, declarators) {
            DeclaratorKindOpeningToken = declaratorKindOpeningToken,
            DeclaratorKindClosingToken = declaratorKindClosingToken,
        };

        int start = declaratorKindOpeningToken?.Start ?? declarators[0].Span.Start;
        int end = declaratorKindClosingToken?.Start ?? declarators[^1].Span.End;
        localDeclaratorList.SetSpan(new(start, end));
        localDeclaratorList.SetLine(declaratorKindOpeningToken?.Line ?? declarators[0].Line);

        return localDeclaratorList;
    }

    public static LocalDeclarator LocalDeclarator (
        Token? fieldKindToken,
        Identifier identifier,
        LocalKind localKind,
        TypeAnnotation? type
    ) {
        var localDeclarator = new LocalDeclarator(identifier, localKind, type) {
            FieldKindToken = fieldKindToken,
        };

        int start = fieldKindToken?.Start ?? identifier.Span.Start;
        int end = type?.Span.End ?? identifier.Span.End;
        localDeclarator.SetSpan(new(start, end));
        localDeclarator.SetLine(fieldKindToken?.Line ?? identifier.Line);

        return localDeclarator;
    }

    public static EqualsValueClause EqualsValueClause (
        Token equalsToken, Expression value
    ) {
        var clause = new EqualsValueClause(value) {
            EqualsToken = equalsToken
        };
        clause.SetSpan(new(clause.EqualsToken.Start, clause.Value.Span.End));
        clause.SetLine(clause.EqualsToken.Line);

        return clause;
    }

    public static TypeAnnotation TypeAnnotation (
        Token colonToken, Identifier identifier
    ) {
        var typeAnnotation = new TypeAnnotation(identifier) {
            ColonToken = colonToken
        };
        typeAnnotation.SetSpan(new(colonToken.Start, identifier.Span.End));
        typeAnnotation.SetLine(colonToken.Line);

        return typeAnnotation;
    }

    public static Operator Operator (Token rawToken, OperatorKind operatorKind) {
        var op = new Operator(operatorKind) {
            RawToken = rawToken,
        };
        op.SetSpan(new(rawToken.Start, rawToken.End));
        op.SetLine(op.RawToken.Line);

        return op;
    }

    public static ParameterList ParameterList (
        Token leftParenToken, List<Parameter> parameters, Token rightParenToken
    ) {
        var paramList = new ParameterList(parameters) {
            LeftParenthesisToken = leftParenToken,
            RightParenthesisToken = rightParenToken,
        };
        paramList.SetSpan(new(leftParenToken.Start, rightParenToken.End));
        paramList.SetLine(leftParenToken.Line);

        return paramList;
    }

    public static Parameter Parameter (
        LocalDeclarator declarator,
        EqualsValueClause? defaultValue
    ) {
        var parameter = new Parameter(declarator, defaultValue);

        int end = defaultValue?.Span.End ?? declarator.Span.End;
        parameter.SetSpan(new(declarator.Span.Start, end));
        parameter.SetLine(declarator.Line);

        return parameter;
    }

    public static MatchCase MatchCase (
        Token? elseToken, List<Expression> tests, Statement consequent
    ) {
        var matchCase = new MatchCase(tests, consequent, tests.Count == 0) {
            ElseToken = elseToken,
        };
        if (elseToken is null) {
            matchCase.SetSpan(new(tests[0].Span.Start, consequent.Span.End));
            matchCase.SetLine(matchCase.Tests[0].Line);
        }
        else {
            matchCase.SetSpan(new(elseToken.Start, consequent.Span.End));
            matchCase.SetLine(elseToken.Line);
        }

        return matchCase;
    }
    
    #region Debug statements
    public static P_PrintStatement PrivPrintStmt (Token p_printToken, Expression expr) {
        var p_printStmt = new P_PrintStatement(expr) {
            P_PrintToken = p_printToken,
        };
        p_printStmt.SetSpan(new(p_printToken.Start, expr.Span.End));
        p_printStmt.SetLine(p_printToken.Line);

        return p_printStmt;
    }
    #endregion
}
