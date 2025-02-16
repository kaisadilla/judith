using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class Identifier : SyntaxNode
{
    private const char ESCAPE_CHAR = '\\';

    public string Name { get; private set; }
    public bool IsEscaped { get; private set; }
    /// <summary>
    /// Metanames are names that the developer can't write, such as "!" or "35".
    /// These names are used by the compiler to generate identifiers that cannot
    /// clash with those defined in the source code.
    /// </summary>
    public bool IsMetaName { get; private set; }

    public Token? RawToken { get; private set; }

    /// <summary>
    /// The symbol this identifier resolved to.
    /// </summary>
    public Symbol? Symbol { get; private set; }

    // TODO: Move this logic to the parser.
    public Identifier(Token rawToken) : base(SyntaxKind.Identifier)
    {
        RawToken = rawToken;
        ExtractIdentifier(rawToken);
    }

    public Identifier(string name, bool isMetaName) : base(SyntaxKind.Identifier)
    {
        Name = name;
        IsMetaName = isMetaName;
        IsEscaped = false;
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.Visit(this);
    }

    public void SetSymbol (Symbol symbol) {
        Symbol = symbol;
    }

    public override string ToString()
    {
        return $"`{RawToken?.Lexeme ?? "<unknown identifier>"}`";
    }

    /// <summary>
    /// Extracts a name and escape flag from the token given and uses it to
    /// set this Identifier's properties.
    /// </summary>
    /// <param name="rawToken">The token to extract data from.</param>
    [MemberNotNull(nameof(Name))]
    private void ExtractIdentifier(Token rawToken)
    {
        if (rawToken.Lexeme.StartsWith(ESCAPE_CHAR))
        {
            Name = rawToken.Lexeme[1..];
            IsEscaped = true;
        }
        else
        {
            Name = rawToken.Lexeme;
            IsEscaped = false;
        }
    }
}
