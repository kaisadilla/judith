using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class Operator : SyntaxNode {
    public Token? RawToken { get; private set; }

    private Operator () : base(SyntaxKind.Operator) { }

    public Operator (Token? rawToken) : this() {
        RawToken = rawToken;
    }

    public override string ToString () {
        return $"{RawToken?.Lexeme ?? "<unknown operator>"}";
    }
}
