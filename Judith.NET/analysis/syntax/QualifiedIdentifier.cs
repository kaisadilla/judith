using Judith.NET.analysis.lexical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class QualifiedIdentifier : Identifier {
    public static readonly OperatorKind[] VALID_OPERATORS = [
        OperatorKind.ScopeResolution,
    ];

    public Identifier Qualifier { get; private init; }
    public Operator Operator { get; private init; }
    public SimpleIdentifier Name { get; private set; }

    public QualifiedIdentifier (
        Identifier qualifier, Operator op, SimpleIdentifier name, bool isMetaName
    )
        : base(SyntaxKind.QualifiedIdentifier, isMetaName)
    {
        if (VALID_OPERATORS.Contains(op.OperatorKind) == false) {
            throw new($"Operator '{op.OperatorKind}' is not valid for this node.");
        }

        Qualifier = qualifier;
        Operator = op;
        Name = name;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }

    public override string FullyQualifiedName () {
        return Qualifier.FullyQualifiedName() + "::" + Name.FullyQualifiedName();
    }
}
