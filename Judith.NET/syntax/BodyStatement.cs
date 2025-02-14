using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public abstract class BodyStatement : Statement {
    protected BodyStatement (SyntaxKind kind) : base(kind) { }
}

public class BlockStatement : BodyStatement {
    public List<SyntaxNode> Nodes { get; init; }
    public Token? OpeningToken { get; init; }
    public Token? ClosingToken { get; init; }

    public BlockStatement (List<SyntaxNode> nodes)
        : base(SyntaxKind.BlockStatement)
    {
        Nodes = nodes;

        Children.AddRange(Nodes);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return "|block> " + Stringify(new {
            Statements = Nodes.Select(stmt => stmt.ToString()),
        }) + " <|";
    }
}

public class ArrowStatement : BodyStatement {
    public Statement Statement { get; init; }
    public Token? ArrowToken { get; init; }

    public ArrowStatement (Statement statement) : base(SyntaxKind.ArrowStatement) {
        Statement = statement;

        Children.Add(Statement);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return "|arrow> " + Statement.ToString() + " <|";
    }
}