using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class FieldInitialization : SyntaxNode {
    public SimpleIdentifier FieldName { get; init; }
    public EqualsValueClause Initializer { get; init; }

    public FieldInitialization (SimpleIdentifier fieldName, EqualsValueClause initializer)
        : base(SyntaxKind.FieldInitialization)    
    {
        FieldName = fieldName;
        Initializer = initializer;

        Children.Add(FieldName, Initializer);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
