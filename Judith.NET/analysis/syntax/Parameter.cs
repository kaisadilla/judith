using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class Parameter : SyntaxNode
{
    public LocalDeclarator Declarator { get; private init; }
    public EqualsValueClause? DefaultValue { get; private init; }

    public Parameter(LocalDeclarator declarator, EqualsValueClause? defaultValue)
        : base(SyntaxKind.Parameter)
    {
        Declarator = declarator;
        DefaultValue = defaultValue;

        Children.Add(declarator);
        if (DefaultValue != null) Children.Add(DefaultValue);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return Declarator.ToString() + (DefaultValue?.ToString() ?? "");
    }
}
