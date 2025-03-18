using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public class SimpleIdentifier : Identifier {
    private const char ESCAPE_CHAR = '\\';

    public string Name { get; private set; }
    public bool IsEscaped { get; private set; }

    public Token? RawToken { get; private set; }

    // TODO: Move this logic to the parser.
    public SimpleIdentifier (Token rawToken)
        : base(SyntaxKind.SimpleIdentifier, false)
    {
        RawToken = rawToken;
        ExtractIdentifier(rawToken);
    }

    public SimpleIdentifier (string name, bool isMetaName)
        : base(SyntaxKind.SimpleIdentifier, isMetaName)
    {
        Name = name;
        IsEscaped = false;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
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
    public override string ToString () {
        return $"{Kind} ({Name}) [Line: {Line}, Span: {Span.Start} - {Span.End}]";
    }

    public override string FullyQualifiedName () {
        return Name;
    }
}
