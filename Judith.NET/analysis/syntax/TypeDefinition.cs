using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis.lexical;

namespace Judith.NET.analysis.syntax;

public abstract class TypeDefinition : Item {
    public Token? TypedefToken { get; set; }
    public bool IsHidden { get; private init; }
    public Token? HidToken { get; init; }

    protected TypeDefinition (SyntaxKind kind, bool isHidden) : base(kind) {
        IsHidden = isHidden;
    }
}
