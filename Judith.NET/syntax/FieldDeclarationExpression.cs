using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public abstract class FieldDeclarationExpression : Expression {
    protected FieldDeclarationExpression (SyntaxKind kind) : base(kind) { }
}

public class SingleFieldDeclarationExpression : FieldDeclarationExpression {
    public FieldDeclarator Declarator { get; init; }
    public EqualsValueClause? Initializer { get; init; }

    public SingleFieldDeclarationExpression (
        FieldDeclarator declarator, EqualsValueClause? initializer
    )
        : base(SyntaxKind.SingleFieldDeclarationExpression)
    {
        Declarator = declarator;
        Initializer = initializer;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        string str = $"{Declarator}";

        if (Initializer != null) {
            str += $" {Initializer}";
        }

        return str;
    }
}

public class MultipleFieldDeclarationExpression : FieldDeclarationExpression {
    public List<FieldDeclarator> Declarators { get; init; }
    public EqualsValueClause? Initializer { get; init; }

    public MultipleFieldDeclarationExpression (
        List<FieldDeclarator> declarators, EqualsValueClause? initializer
    )
        : base(SyntaxKind.MultipleFieldDeclarationExpression)
    {
        Declarators = declarators;
        Initializer = initializer;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        string str = $"({string.Join(", ", Declarators)})";

        if (Initializer != null) {
            str += $" {Initializer}";
        }

        return str;
    }
}