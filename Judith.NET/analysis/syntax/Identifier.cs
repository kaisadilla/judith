using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public abstract class Identifier : SyntaxNode {

    /// <summary>
    /// Metanames are names that the developer can't write, such as "!" or "35".
    /// These names are used by the compiler to generate identifiers that cannot
    /// clash with those defined in the source code.
    /// </summary>
    public bool IsMetaName { get; private set; }

    protected Identifier (SyntaxKind kind, bool isMetaName) : base(kind) {
        IsMetaName = isMetaName;
    }

    public abstract string FullyQualifiedName ();
}
