using Judith.NET.analysis.lexical;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public abstract class Body : SyntaxNode {
    protected Body (SyntaxKind kind) : base(kind) { }
}

public class BlockBody : Body {
    public List<SyntaxNode> Nodes { get; private init; }
    public Token? OpeningToken { get; init; }
    public Token? ClosingToken { get; init; }

    public BlockBody (List<SyntaxNode> nodes) : base(SyntaxKind.BlockBody) {
        Nodes = nodes;

        Children.AddRange(Nodes);
    }

    public void AppendNode (SyntaxNode node) {
        Nodes.Add(node);
        Children.Add(node);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}

public class ArrowBody : Body {
    public Expression Expression { get; private init; }
    public Token? ArrowToken { get; init; }

    public ArrowBody (Expression expr) : base(SyntaxKind.ArrowBody) {
        Expression = expr;

        Children.Add(Expression);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}

public class ExpressionBody : Body {
    public Expression Expression { get; private init; }

    public ExpressionBody (Expression expression) : base(SyntaxKind.ExpressionBody) {
        Expression = expression;

        Children.Add(Expression);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}