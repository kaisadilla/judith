using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;
public abstract class Statement : SyntaxNode {
    protected Statement (SyntaxKind kind) : base(kind) { }
}
