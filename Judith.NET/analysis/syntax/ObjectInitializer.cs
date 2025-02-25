using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class ObjectInitializer : SyntaxNode {
    public List<AssignmentExpression> Assignments { get; private init; }

    public Token? LeftBracketToken { get; set; }
    public Token? RightBracketToken { get; set; }

    public ObjectInitializer (List<AssignmentExpression> assignments)
        : base(SyntaxKind.ObjectInitializer)
    {
        Assignments = assignments;

        Children.AddRange(Assignments);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}

