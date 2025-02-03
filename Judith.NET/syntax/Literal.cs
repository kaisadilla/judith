using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;
public class Literal : SyntaxNode {
    public Token? RawToken { get; init; }

    public LiteralKind LiteralKind { get; init; }

    private Literal () : base(SyntaxKind.Literal) { }

    public Literal (Token? rawToken) : this () {
        RawToken = rawToken;
    }

    public override string ToString () {
        return $"{RawToken?.Lexeme ?? "<unknown literal>"}";
    }
}
