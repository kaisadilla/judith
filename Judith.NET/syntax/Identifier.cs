using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class Identifier : SyntaxNode {
    private const char ESCAPE_CHAR = '\\';

    public string Name { get; private set; }
    public bool IsEscaped { get; private set; }

    public Token? RawToken { get; private set; }

    // TODO: Move this logic to the parser.
    public Identifier (Token rawToken) : base(SyntaxKind.Identifier) {
        RawToken = rawToken;
        ExtractIdentifier(rawToken);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return $"`{RawToken?.Lexeme ?? "<unknown identifier>"}`";
    }

    /// <summary>
    /// Extracts a name and escape flag from the token given and uses it to
    /// set this Identifier's properties.
    /// </summary>
    /// <param name="rawToken">The token to extract data from.</param>
    [MemberNotNull(nameof(Name))]
    private void ExtractIdentifier (Token rawToken) {
        if (rawToken.Lexeme.StartsWith(ESCAPE_CHAR)) {
            Name = rawToken.Lexeme[1..];
            IsEscaped = true;
        }
        else {
            Name = rawToken.Lexeme;
            IsEscaped = false;
        }
    }
}
