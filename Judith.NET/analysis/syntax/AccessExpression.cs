using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class AccessExpression : Expression {
    public Expression? Receiver { get; private init; }
    public Operator Operator { get; private init; }
    public Identifier Member { get; private init; }

    public AccessKind AccessKind { get; private init; }

    public AccessExpression (Expression? receiver, Operator op, Identifier member)
        : base(SyntaxKind.AccessExpression
    ) {
        Receiver = receiver;
        Operator = op;
        Member = member;

        if (op.OperatorKind == OperatorKind.MemberAccess) {
            AccessKind = AccessKind.Member;
        }
        else if (op.OperatorKind == OperatorKind.ScopeResolution) {
            AccessKind = AccessKind.ScopeResolution;
        }
        else {
            throw new($"Invalid access operator: '{op.OperatorKind}'.");
        }

        if (Receiver != null) Children.Add(Receiver);
        Children.Add(Operator, Member);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}
