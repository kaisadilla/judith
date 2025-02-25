using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class ObjectInitializationExpression : Expression {
    public Expression? Provider { get; private set; }
    public ObjectInitializer Initializer { get; private set; }

    public ObjectInitializationExpression (
        Expression? provider, ObjectInitializer initializer
    )
        : base(SyntaxKind.ObjectInitializationExpression)
    {
        Provider = provider;
        Initializer = initializer;

        if (Provider != null) Children.Add(Provider);
        Children.Add(Initializer);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
