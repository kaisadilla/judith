using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;
public static class SyntaxFactory {
    public static FunctionDefinition FunctionDefinition (
        Token? hidToken,
        Token funcToken,
        SimpleIdentifier name,
        ParameterList parameters,
        TypeAnnotation? returnType,
        BlockBody body
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

    public static StructTypeDefinition StructTypeDefinition (
        Token? hidToken,
        Token typedefToken,
        Token structToken,
        SimpleIdentifier identifier,
        List<MemberField> memberFields,
        Token endToken
    ) {
        var isHidden = hidToken != null;

        var structTypedef = new StructTypeDefinition(
            isHidden, identifier, memberFields
        ) {
            TypedefToken = typedefToken,
            StructToken = structToken,
            EndToken = endToken,
        };

        structTypedef.SetSpan(new(typedefToken.Start, endToken.End));
        structTypedef.SetLine(typedefToken.Line);

        return structTypedef;
    }

    public static BlockBody BlockBody (
        Token? openingToken, List<SyntaxNode> statements, Token closingToken
    ) {
        var blockBody = new BlockBody(statements) {
            OpeningToken = openingToken,
            ClosingToken = closingToken,
        };

        if (openingToken != null) {
            blockBody.SetSpan(new(openingToken.Start, closingToken.End));
            blockBody.SetLine(openingToken.Line);
        }
        else if (statements.Count > 0) {
            blockBody.SetSpan(new(statements[0].Span.Start, closingToken.End));
            blockBody.SetLine(statements[0].Line);
        }
        else {
            blockBody.SetSpan(new(closingToken.Start, closingToken.End));
            blockBody.SetLine(closingToken.Line);
        }

        return blockBody;
    }

    public static ArrowBody ArrowBody (
        Token arrowToken, Expression expression
    ) {
        var arrowBody = new ArrowBody(expression) {
            ArrowToken = arrowToken,
        };
        arrowBody.SetSpan(new(arrowToken.Start, expression.Span.End));
        arrowBody.SetLine(arrowToken.Line);

        return arrowBody;
    }

    public static ExpressionBody ExpressionBody (Expression expression) {
        var exprBody = new ExpressionBody(expression);
        exprBody.SetSpan(new(expression.Span.Start, expression.Span.End));
        exprBody.SetLine(expression.Line);

        return exprBody;
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
        Token ifToken, Expression test, Body consequent
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
        Body consequent,
        Token elseToken,
        Body alternate
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

    public static LoopExpression LoopExpression (Token loopToken, Body body) {
        var loopExpr = new LoopExpression(body) {
            LoopToken = loopToken,
        };
        loopExpr.SetSpan(new(loopToken.Start, body.Span.End));
        loopExpr.SetLine(loopToken.Line);

        return loopExpr;
    }

    public static WhileExpression WhileExpression (
        Token whileToken, Expression test, Body body
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
        Body body
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

    public static AccessExpression AccessExpression (
        Expression? receiver, Operator op, SimpleIdentifier member
    ) {
        var accessExpr = new AccessExpression(receiver, op, member);
        accessExpr.SetSpan(new(receiver?.Span.Start ?? op.Span.Start, member.Span.End));
        accessExpr.SetLine(receiver?.Line ?? op.Line);

        return accessExpr;
    }

    public static CallExpression CallExpression (
        Expression callee, ArgumentList argList
    ) {
        var callExpr = new CallExpression(callee, argList);
        callExpr.SetSpan(new(callee.Span.Start, argList.Span.End));
        callExpr.SetLine(callee.Line);

        return callExpr;
    }

    public static ObjectInitializationExpression ObjectInitializationExpression (
        Expression? provider, ObjectInitializer initializer
    ) {
        var initExpr = new ObjectInitializationExpression(provider, initializer);
        initExpr.SetSpan(new(
            provider?.Span.Start ?? initializer.Span.Start,
            initializer.Span.End
        ));
        initExpr.SetLine(provider?.Line ?? initializer.Line);

        return initExpr;
    }

    public static FieldInitialization FieldInitialization (
        SimpleIdentifier fieldName, EqualsValueClause initializer
    ) {
        var fieldInit = new FieldInitialization(fieldName, initializer);
        fieldInit.SetSpan(new(fieldName.Span.Start, initializer.Span.End));
        fieldInit.SetLine(fieldName.Line);

        return fieldInit;
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

    public static QualifiedIdentifier QualifiedIdentifier (
        Identifier qualifier, Operator op, SimpleIdentifier identifier
    ) {
        var qualifiedId = new QualifiedIdentifier(qualifier, op, identifier, false);
        qualifiedId.SetSpan(new(qualifier.Span.Start, identifier.Span.End));
        qualifiedId.SetLine(qualifier.Line);

        return qualifiedId;
    }

    public static SimpleIdentifier SimpleIdentifier (Token rawToken) {
        var id = new SimpleIdentifier(rawToken);
        id.SetSpan(new(rawToken.Start, rawToken.End));
        id.SetLine(rawToken.Line);

        return id;
    }

    public static Literal Literal (Token rawToken) {
        var lit = new Literal(rawToken);
        lit.SetSpan(new(rawToken.Start, rawToken.End));
        lit.SetLine(rawToken.Line);

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
        SimpleIdentifier identifier,
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
        Token equalsToken, List<Expression> values
    ) {
        if (values.Count == 0) {
            throw new("EqualsValueClause must have at least one value.");
        }

        var clause = new EqualsValueClause(values) {
            EqualsToken = equalsToken
        };
        clause.SetSpan(new(clause.EqualsToken.Start, clause.Values[^1].Span.End));
        clause.SetLine(clause.EqualsToken.Line);

        return clause;
    }

    public static TypeAnnotation TypeAnnotation (Token colonToken, TypeNode type) {
        var typeAnnotation = new TypeAnnotation(type) {
            Delimiter = colonToken
        };
        typeAnnotation.SetSpan(new(colonToken.Start, type.Span.End));
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

    public static ArgumentList ArgumentList (
        Token leftParenToken, List<Argument> arguments, Token rightParenToken
    ) {
        var argList = new ArgumentList(arguments) {
            LeftParenthesisToken = leftParenToken,
            RightParenthesisToken = rightParenToken,
        };
        argList.SetSpan(new(leftParenToken.Start, rightParenToken.End));
        argList.SetLine(leftParenToken.Line);

        return argList;
    }

    public static Argument Argument (Expression expr) {
        var argument = new Argument(expr);

        argument.SetSpan(new(expr.Span.Start, expr.Span.End));
        argument.SetLine(expr.Line);

        return argument;
    }

    public static MatchCase MatchCase (
        Token? elseToken, List<Expression> tests, Body consequent
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

    public static ObjectInitializer ObjectInitializer (
        Token leftBracketToken,
        List<FieldInitialization> fieldInitializations,
        Token rightBracketToken
    ) {
        var objInit = new ObjectInitializer(fieldInitializations) {
            LeftBracketToken = leftBracketToken,
            RightBracketToken = rightBracketToken,
        };
        objInit.SetSpan(new(leftBracketToken.Start, rightBracketToken.End));
        objInit.SetLine(leftBracketToken.Line);

        return objInit;
    }

    public static MemberField MemberField (
        Token? accessToken,
        Token? staticToken,
        Token? mutableToken,
        Token? constToken,
        SimpleIdentifier identifier,
        TypeAnnotation typeAnnotation,
        EqualsValueClause? initializer
    ) {
        MemberAccessKind access = accessToken?.Kind switch {
            TokenKind.KwPub => MemberAccessKind.Public,
            TokenKind.KwHid => MemberAccessKind.Hidden,
            _ => MemberAccessKind.ReadOnly,
        };

        bool isStatic = staticToken != null;
        bool isMutable = mutableToken != null;
        bool isConst = constToken != null;

        var memberField = new MemberField(
            access, isStatic, isMutable, isConst, identifier, typeAnnotation, initializer
        ) {
            AccessToken = accessToken,
            StaticToken = staticToken,
            MutableToken = mutableToken,
            ConstToken = constToken,
        };

        int start = true switch {
            true when accessToken != null => accessToken.Start,
            true when staticToken != null => staticToken.Start,
            true when mutableToken != null => mutableToken.Start,
            _ => identifier.Span.Start,
        };

        int line = true switch {
            true when accessToken != null => accessToken.Line,
            true when staticToken != null => staticToken.Line,
            true when mutableToken != null => mutableToken.Line,
            _ => identifier.Line,
        };

        int end = initializer?.Span.End ?? typeAnnotation.Span.End;

        memberField.SetSpan(new(start, end));
        memberField.SetLine(line);

        return memberField;
    }

    public static UnionType UnionType (List<TypeNode> memberTypes) {
        var unionType = new UnionType(false, false, memberTypes);
        unionType.SetSpan(new(memberTypes[0].Span.Start, memberTypes[^1].Span.End));
        unionType.SetLine(memberTypes[0].Line);

        return unionType;
    }

    public static RawArrayType RawArrayType (
        TypeNode memberType,
        Token leftSqBracketToken,
        Expression length,
        Token rightSqBracketToken
    ) {
        var rawArrayType = new RawArrayType(false, false, memberType, length) {
            LeftSquareBracketToken = leftSqBracketToken,
            RightSquareBracketToken = rightSqBracketToken,
        };
        rawArrayType.SetSpan(new(memberType.Span.Start, rightSqBracketToken.End));
        rawArrayType.SetLine(memberType.Line);

        return rawArrayType;
    }

    public static GroupType GroupType (
        Token leftParenToken, TypeNode type, Token rightParenToken
    ) {
        var groupType = new GroupType(false, false, type) {
            LeftParenthesisToken = leftParenToken,
            RightParenthesisToken = rightParenToken,
        };
        groupType.SetSpan(new(leftParenToken.Start, rightParenToken.End));
        groupType.SetLine(leftParenToken.Line);

        return groupType;
    }

    public static FunctionType FunctionType (
        Token leftParenToken,
        List<TypeNode> paramTypes,
        Token rightParenToken,
        TypeNode returnType
    ) {
        var funcType = new FunctionType(false, false, paramTypes, returnType) {
            LeftParenthesisToken = leftParenToken,
            RightParenthesisToken = rightParenToken,
        };
        funcType.SetSpan(new(leftParenToken.Start, returnType.Span.End));
        funcType.SetLine(leftParenToken.Line);

        return funcType;
    }

    public static TupleArrayType TupleArrayType (
        Token leftSqBracket, List<TypeNode> memberTypes, Token rightSqBracket
    ) {
        var arrType = new TupleArrayType(false, false, memberTypes) {
            LeftSquareBracketToken = leftSqBracket,
            RightSquareBracketToken = rightSqBracket,
        };
        arrType.SetSpan(new(leftSqBracket.Start, rightSqBracket.End));
        arrType.SetLine(leftSqBracket.Line);

        return arrType;
    }

    public static LiteralType LiteralType (Literal literal) {
        var literalType = new LiteralType(false, false, literal);
        literalType.SetSpan(literal.Span);
        literalType.SetLine(literal.Line);

        return literalType;
    }

    public static IdentifierType IdentifierType (Identifier id) {
        var idType = new IdentifierType(false, false, id);
        idType.SetSpan(id.Span);
        idType.SetLine(id.Line);

        return idType;
    }

    public static TypeNode SetTypeConstness (TypeNode type, Token? constToken) {
        type.SetConstant(constToken != null);
        type.SetSpan(new(constToken?.Start ?? type.Span.Start, type.Span.End));
        type.SetLine(constToken?.Line ?? type.Line);
        
        return type;
    }

    public static TypeNode SetNullability (TypeNode type, Token? questionToken) {
        type.SetNullable(questionToken != null);
        type.SetSpan(new(type.Span.Start, questionToken?.End ?? type.Span.End));

        return type;
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
