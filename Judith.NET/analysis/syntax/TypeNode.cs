using Judith.NET.analysis.lexical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public abstract class TypeNode : SyntaxNode {
    public bool IsConstant { get; protected set; }
    public bool IsNullable { get; protected set; }

    public Token? ConstToken { get; protected init; }
    public Token? NullableToken { get; protected init; }

    protected TypeNode (SyntaxKind kind, bool isConstant, bool isNullable) : base(kind) {
        IsConstant = isConstant;
        IsNullable = isNullable;
    }

    public void SetConstant (bool isConstant) => IsConstant = isConstant;
    public void SetNullable (bool isNullable) => IsNullable = isNullable;
}
