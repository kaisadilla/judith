using Judith.NET.analysis.binder;
using Judith.NET.analysis.syntax;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.analyzers;

public class BlockTypeResolver : SyntaxVisitor<TypeInfo?> {
    public MessageContainer Messages { get; private set; } = new();

    private Compilation _cmp;
    private ScopeResolver _scope;

    public BlockTypeResolver (Compilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp.Binder, _cmp.SymbolTable);
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    public override TypeInfo? Visit (FunctionDefinition node) {
        if (node.ReturnTypeAnnotation != null) {
            return null;
        }

        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundFunctionDefinition>(node);
        if (TypeInfo.IsResolved(boundNode.ReturnType)) {
            return null;
        }

        var returnType = Visit(node.Body);

        if (returnType != null) {
            boundNode.ReturnType = returnType;
            boundNode.Symbol.Type = returnType;
        }
        else {
            boundNode.ReturnType = TypeInfo.UnresolvedType;
            boundNode.Symbol.Type = TypeInfo.UnresolvedType;
        }

        return boundNode.ReturnType;
    }

    public override TypeInfo Visit (BlockStatement node) {
        TypeInfo returnType = TypeInfo.UnresolvedType;

        foreach (var stmt in node.Nodes) {
            if (stmt.Kind != SyntaxKind.ReturnStatement) continue;

            var type = Visit(stmt);
            if (TypeInfo.IsResolved(type) == false) continue;

            if (TypeInfo.IsResolved(returnType) && type != returnType) {
                throw new NotImplementedException(
                    "Multiple return types not implemented yet."
                );
            }

            returnType = type;
        }

        return returnType;
    }

    public override TypeInfo? Visit (ReturnStatement node) {
        if (node.Expression == null) return TypeInfo.VoidType;

        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);

        return boundNode.Type;
    }
}
