using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public abstract class TypeDefinition : Item {
    public Token? TypedefToken { get; set; }

    protected TypeDefinition (SyntaxKind kind) : base(kind) {}
}
