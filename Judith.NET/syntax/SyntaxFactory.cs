using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;
public static class SyntaxFactory {
    public static Operator Operator (Token rawToken) {
        var op = new Operator(rawToken);
        op.SetSpan(new(rawToken.Start, rawToken.End));

        return op;
    }

    public static Identifier Identifier (Token rawToken) {
        var id = new Identifier(rawToken);
        id.SetSpan(new(rawToken.Start, rawToken.End));

        return id;
    }

    public static Literal Literal (Token rawToken) {
        var lit = new Literal(rawToken);
        lit.SetSpan(new(rawToken.Start, rawToken.End));

        return lit;
    }

    public static FieldDeclarator FieldDeclarator (
        Token? fieldKindToken,
        Identifier identifier,
        FieldKind fieldKind,
        IdentifierExpression? type
    ) {
        var decl = new FieldDeclarator(identifier, fieldKind, type) {
            KindToken = fieldKindToken,
        };

        int start = decl.KindToken?.Start ?? decl.Identifier.Span.Start;
        int end = decl.Type?.Span.End ?? decl.Identifier.Span.End;
        decl.SetSpan(new(start, end));

        return decl;
    }

    public static IdentifierExpression IdentifierExpression (Identifier identifier) {
        var idExpr = new IdentifierExpression(identifier);
        idExpr.SetSpan(identifier.Span);

        return idExpr;
    }

    public static LiteralExpression LiteralExpression (Literal literal) {
        var litExpr = new LiteralExpression(literal);
        litExpr.SetSpan(literal.Span);

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

        return groupExpr;
    }

    public static LeftUnaryExpression LeftUnaryExpression (
        Operator op, Expression expr
    ) {
        var unaryExpr = new LeftUnaryExpression(op, expr);
        unaryExpr.SetSpan(new(op.Span.Start, expr.Span.End));

        return unaryExpr;
    }

    public static BinaryExpression BinaryExpression (
        Expression left, Operator op, Expression right
    ) {
        var binaryExpr = new BinaryExpression(op, left, right);
        binaryExpr.SetSpan(new(left.Span.Start, right.Span.End));

        return binaryExpr;
    }

    public static SingleFieldDeclarationExpression SingleFieldDeclarationExpression (
        FieldDeclarator declarator
    ) {
        var declExpr = new SingleFieldDeclarationExpression(declarator);

        int start = declExpr.Declarator.Span.Start;
        int end = declExpr.Declarator.Span.End; // TODO: Initializer.
        declExpr.SetSpan(new(start, end));

        return declExpr;
    }

    public static MultipleFieldDeclarationExpression MultipleFieldDeclarationExpression (
        List<FieldDeclarator> declarators
    ) {
        if (declarators.Count == 0) throw new Exception(
            "There must be at least one declarator in a " +
            "multiple variable declaration statement."
        );

        var declExpr = new MultipleFieldDeclarationExpression(declarators);
        
        int start = declExpr.Declarators[0].Span.Start;
        int end = declExpr.Declarators[^1].Span.End; // TODO: Initializer.
        declExpr.SetSpan(new(start, end));

        return declExpr;
    }

    public static LocalDeclarationStatement LocalDeclarationStatement (
        FieldDeclarationExpression declaration
    ) {
        var declStmt = new LocalDeclarationStatement(declaration);
        declStmt.SetSpan(declStmt.Declaration.Span);

        return declStmt;
    }
}
