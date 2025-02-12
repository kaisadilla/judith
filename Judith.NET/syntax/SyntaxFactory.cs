using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;
public static class SyntaxFactory {
    public static Operator Operator (Token rawToken, OperatorKind operatorKind) {
        var op = new Operator(operatorKind) {
            RawToken = rawToken,
        };
        op.SetSpan(new(rawToken.Start, rawToken.End));
        op.SetLine(op.RawToken.Line);

        return op;
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

    public static FieldDeclarator FieldDeclarator (
        Token? fieldKindToken,
        Identifier identifier,
        FieldKind fieldKind,
        IdentifierExpression? type
    ) {
        var decl = new FieldDeclarator(identifier, fieldKind, type) {
            FieldKindToken = fieldKindToken,
        };

        int start = decl.FieldKindToken?.Start ?? decl.Identifier.Span.Start;
        int end = decl.Type?.Span.End ?? decl.Identifier.Span.End;
        decl.SetSpan(new(start, end));
        decl.SetLine(fieldKindToken?.Line ?? identifier.Line);

        return decl;
    }

    public static EqualsValueClause EqualsValueClause (
        Expression value, Token equalsToken
    ) {
        var clause = new EqualsValueClause(value) {
            EqualsToken = equalsToken
        };
        clause.SetSpan(new(clause.EqualsToken.Start, clause.Value.Span.End));
        clause.SetLine(clause.EqualsToken.Line);

        return clause;
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

    public static GroupExpression GroupExpression (
        Expression expr, Token leftParen, Token rightParen
    ) {
        var groupExpr = new GroupExpression(expr) {
            LeftParenthesisToken = leftParen,
            RightParenthesisToken = rightParen,
        };
        groupExpr.SetSpan(new(leftParen.Start, rightParen.End));
        groupExpr.SetLine(groupExpr.LeftParenthesisToken.Line);

        return groupExpr;
    }

    public static LeftUnaryExpression LeftUnaryExpression (
        Operator op, Expression expr
    ) {
        var unaryExpr = new LeftUnaryExpression(op, expr);
        unaryExpr.SetSpan(new(op.Span.Start, expr.Span.End));
        unaryExpr.SetLine(unaryExpr.Operator.Line);

        return unaryExpr;
    }

    public static BinaryExpression BinaryExpression (
        Expression left, Operator op, Expression right
    ) {
        var binaryExpr = new BinaryExpression(op, left, right);
        binaryExpr.SetSpan(new(left.Span.Start, right.Span.End));
        binaryExpr.SetLine(binaryExpr.Left.Line);

        return binaryExpr;
    }

    public static AssignmentExpression AssignmentExpression (
        Expression left, Token equalsToken, Expression right
    ) {
        var assignExpr = new AssignmentExpression(left, right) {
            EqualsToken = equalsToken,
        };
        assignExpr.SetSpan(new(assignExpr.Left.Span.Start, assignExpr.Right.Span.End));
        assignExpr.SetLine(assignExpr.Left.Line);

        return assignExpr;
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
        FieldDeclarationExpression initializer,
        Token inToken,
        Expression enumerable,
        Statement body
    ) {
        var foreachExpr = new ForeachExpression(initializer, enumerable, body) {
            ForeachToken = foreachToken,
            InToken = inToken,
        };
        foreachExpr.SetSpan(new(foreachToken.Start, body.Span.End));
        foreachExpr.SetLine(foreachToken.Line);

        return foreachExpr;
    }

    public static SingleFieldDeclarationExpression SingleFieldDeclarationExpression (
        FieldDeclarator declarator, EqualsValueClause? initializer
    ) {
        var declExpr = new SingleFieldDeclarationExpression(declarator, initializer);

        int start = declExpr.Declarator.Span.Start;
        int end = declExpr.Declarator.Span.End; // TODO: Initializer.
        declExpr.SetSpan(new(start, end));
        declExpr.SetLine(declarator.Line);

        return declExpr;
    }

    public static MultipleFieldDeclarationExpression MultipleFieldDeclarationExpression (
        List<FieldDeclarator> declarators, EqualsValueClause? initializer
    ) {
        if (declarators.Count == 0) throw new Exception(
            "There must be at least one declarator in a " +
            "multiple variable declaration statement."
        );

        var declExpr = new MultipleFieldDeclarationExpression(declarators, initializer);
        
        int start = declExpr.Declarators[0].Span.Start;
        int end = declExpr.Declarators[^1].Span.End; // TODO: Initializer.
        declExpr.SetSpan(new(start, end));
        declExpr.SetLine(declExpr.Declarators[0].Line);

        return declExpr;
    }

    public static LocalDeclarationStatement LocalDeclarationStatement (
        FieldDeclarationExpression declaration
    ) {
        var declStmt = new LocalDeclarationStatement(declaration);
        declStmt.SetSpan(declStmt.Declaration.Span);
        declStmt.SetLine(declStmt.Declaration.Line);

        return declStmt;
    }

    public static ExpressionStatement ExpressionStatement (Expression expr) {
        var exprStmt = new ExpressionStatement(expr);
        exprStmt.SetSpan(expr.Span);
        exprStmt.SetLine(expr.Line);

        return exprStmt;
    }

    public static BlockStatement BlockStatement (
        Token openingToken, List<Statement> statements, Token closingToken
    ) {
        var blockStmt = new BlockStatement(statements) {
            OpeningToken = openingToken,
            ClosingToken = closingToken,
        };
        blockStmt.SetSpan(new(openingToken.Start, closingToken.End));
        blockStmt.SetLine(openingToken.Line);

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

    public static FunctionItem FunctionItem (
        Token funcToken,
        Identifier name,
        ParameterList parameters,
        IdentifierExpression? returnType,
        Statement body
    ) {
        var funcItem = new FunctionItem(name, parameters, returnType, body) {
            FuncToken = funcToken,
        };
        funcItem.SetSpan(new(funcToken.Start, body.Span.End));
        funcItem.SetLine(funcToken.Line);

        return funcItem;
    }

    public static ParameterList ParameterList(
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
        Token? fieldKindToken,
        Identifier identifier,
        FieldKind fieldKind,
        IdentifierExpression? type,
        EqualsValueClause? defaultValue
    ) {
        var parameter = new Parameter(identifier, fieldKind, type, defaultValue) {
            FieldKindToken = fieldKindToken,
        };

        int start = parameter.FieldKindToken?.Start ?? parameter.Identifier.Span.Start;
        int end = true switch { // I oppose C# allowing this kind of esoteric expressions.
            true when defaultValue is not null => defaultValue.Span.End,
            true when type is not null => type.Span.End,
            _ => identifier.Span.End,
        };

        parameter.SetSpan(new(start, end));
        parameter.SetLine(parameter.FieldKindToken?.Line ?? parameter.Identifier.Line);

        return parameter;
    }

    public static PrivPrintStmt PrivPrintStmt (Token p_printToken, Expression expr) {
        var p_printStmt = new PrivPrintStmt(expr) {
            P_PrintToken = p_printToken,
        };
        p_printStmt.SetSpan(new(p_printToken.Start, expr.Span.End));
        p_printStmt.SetLine(p_printToken.Line);

        return p_printStmt;
    }
}
