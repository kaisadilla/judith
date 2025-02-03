using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class LocalDeclarationStatement : Statement {
    public FieldDeclarationExpression Declaration { get; init; }

    public LocalDeclarationStatement (FieldDeclarationExpression declaration)
        : base(SyntaxKind.LocalDeclarationStatement)
    {
        Declaration = declaration;
    }

    public override string ToString () {
        return $"{Declaration}";
    }
}
