using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class LocalDeclaratorList : SyntaxNode {
    public LocalDeclaratorKind DeclaratorKind { get; private init; }
    public List<LocalDeclarator> Declarators { get; private init; }

    public Token? DeclaratorKindOpeningToken { get; init; }
    public Token? DeclaratorKindClosingToken { get; init; }

    public LocalDeclaratorList (
        LocalDeclaratorKind declaratorKind, List<LocalDeclarator> declarators
    )
        : base(SyntaxKind.LocalDeclaratorList) {
        DeclaratorKind = declaratorKind;
        Declarators = declarators;

        foreach (var declarator in declarators) {
            Children.Add(declarator);
        }
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }

    public override string ToString () {
        return Stringify(Declarators.Select(d => d.ToString()));
    }
}
