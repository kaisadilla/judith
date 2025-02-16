using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class Operator : SyntaxNode {
    public OperatorKind OperatorKind { get; private init; }

    public Token? RawToken { get; init; }

    private Operator () : base(SyntaxKind.Operator) { }

    public Operator (OperatorKind operatorKind) : this() {
        OperatorKind = operatorKind;
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }

    public override string ToString () {
        return $"{RawToken?.Lexeme ?? "<unknown operator>"}";
    }
}
