using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public abstract class Item : SyntaxNode
{
    protected Item(SyntaxKind kind) : base(kind) { }
}
