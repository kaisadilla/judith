using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class Identifier : SyntaxNode {
    public Token? RawToken { get; private set; }

    public Identifier (Token rawToken) : base(SyntaxKind.Identifier) {
        RawToken = rawToken;
    }

    public override string ToString () {
        return $"`{RawToken?.Lexeme ?? "<unknown identifier>"}`";
    }
}
