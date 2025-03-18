using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public abstract class TypeNode : SyntaxNode {
    public TypeNode (SyntaxKind kind) : base(kind) {}
}
