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
    public List<Statement> Statements { get; init; }
    public Token? OpeningToken { get; init; }
    public Token? ClosingToken { get; init; }

    public BlockStatement (List<Statement> statements)
        : base(SyntaxKind.BlockStatement)
    {
        Statements = statements;
    }

    public override string ToString () {
        return "|block> " + Stringify(new {
            Statements = Statements.Select(stmt => stmt.ToString()),
        }) + " <|";
    }
}

public class ArrowStatement : BodyStatement {
    public Statement Statement { get; init; }
    public Token? ArrowToken { get; init; }

    public ArrowStatement (Statement statement) : base(SyntaxKind.ArrowStatement) {
        Statement = statement;
    }

    public override string ToString () {
        return "|arrow> " + Statement.ToString() + " <|";
    }
}