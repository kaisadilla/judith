using Judith.NET.analysis.binder;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.analyzers;

public class TypeTableBuilder : SyntaxVisitor {
    private Compilation _cmp;
    private ScopeResolver _scope;

    public TypeTableBuilder (Compilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp.Binder, _cmp.SymbolTable);
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    public override void Visit (StructTypeDefinition node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundStructTypeDefinition>(node);

        var type = new TypeInfo(
            TypeKind.Struct,
            boundNode.Symbol.Name,
            boundNode.Symbol.FullyQualifiedName
        );

        _cmp.TypeTable.AddType(type);

        _scope.BeginScope(node);
        foreach (var field in node.MemberFields) {
            Visit(field);
        }
        _scope.EndScope();

        boundNode.Symbol.Type = TypeInfo.NoType;
        boundNode.Type = TypeInfo.NoType;
        boundNode.Symbol.AssociatedType = type;
    }
}
