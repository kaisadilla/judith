using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.semantics;

public class TypeSymbol : Symbol {
    public List<MemberSymbol> MemberFields { get; private set; } = [];

    public TypeSymbol (
        SymbolKind kind, string name, string fullyQualifiedName, string assembly
    )
        : base(kind, name, fullyQualifiedName, assembly)
    {}

    public static TypeSymbol FreeSymbol (SymbolKind kind, string name) {
        return new(kind, name, name, "");
    }

    public bool TryGetMember (
        string memberName, [NotNullWhen(true)] out MemberSymbol? member
    ) {
        member = MemberFields.Find(m => m.Name == memberName);
        return member != null;
    }

    public static bool IsResolved ([NotNullWhen(true)] TypeSymbol? symbol) {
        return symbol != null && symbol.Kind != SymbolKind.UnresolvedPseudoType;
    }
}