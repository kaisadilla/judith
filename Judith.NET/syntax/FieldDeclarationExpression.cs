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

    public SingleFieldDeclarationExpression (
        FieldDeclarator declarator
    )
        : base(SyntaxKind.SingleFieldDeclarationExpression)
    {
        Declarator = declarator;
    }

    public override string ToString () {
        return $"{Declarator}";
    }
}

public class MultipleFieldDeclarationExpression : FieldDeclarationExpression {
    public List<FieldDeclarator> Declarators { get; init; }

    public MultipleFieldDeclarationExpression (
        List<FieldDeclarator> declarators
    )
        : base(SyntaxKind.MultipleFieldDeclarationExpression)
    {
        Declarators = declarators;
    }

    public override string ToString () {
        return $"({string.Join(", ", Declarators)})";
    }
}