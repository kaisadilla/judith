using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class LiteralExpression : Expression {
    public Literal Literal { get; private set; }

    public LiteralExpression (Literal literal) : base(SyntaxKind.LiteralExpression) {
        Literal = literal;
    }

    public override string ToString () {
        return $"{Literal}";
    }
}
