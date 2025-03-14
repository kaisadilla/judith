﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public abstract class BodyStatement : Statement {
    protected BodyStatement (SyntaxKind kind) : base(kind) { }
}

public class BlockStatement : BodyStatement {
    public List<SyntaxNode> Nodes { get; init; }
    public Token? OpeningToken { get; init; }
    public Token? ClosingToken { get; init; }

    public BlockStatement (List<SyntaxNode> nodes)
        : base(SyntaxKind.BlockStatement) {
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

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}