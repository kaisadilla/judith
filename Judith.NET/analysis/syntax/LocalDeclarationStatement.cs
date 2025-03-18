using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public class LocalDeclarationStatement : Statement {
    public LocalDeclaratorList DeclaratorList { get; private init; }
    public EqualsValueClause? Initializer { get; private init; }

    /// <summary>
    /// The "const" or "var" token that starts this local declaration statement.
    /// </summary>
    public Token? DeclaratorToken { get; init; }

    public LocalDeclarationStatement (
        LocalDeclaratorList declaratorList, EqualsValueClause? initializer
    )
        : base(SyntaxKind.LocalDeclarationStatement)
    {
        DeclaratorList = declaratorList;
        Initializer = initializer;

        Children.Add(DeclaratorList);
        if (Initializer != null) Children.Add(Initializer);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}

