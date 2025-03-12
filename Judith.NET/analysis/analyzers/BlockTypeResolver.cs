using Judith.NET.analysis.binder;
using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.analyzers;

using RetInfo = (SyntaxKind retKind, TypeSymbol type);

public class BlockTypeResolver : SyntaxVisitor<RetInfo?> {
    public MessageContainer Messages { get; private set; } = new();

    private Compilation _cmp;
    private ScopeResolver _scope;

    public BlockTypeResolver (Compilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp);
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    //public override TypeInfo? Visit (FunctionDefinition node) {
    //    if (node.ReturnTypeAnnotation != null) {
    //        return null;
    //    }
    //
    //    var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundFunctionDefinition>(node);
    //    if (TypeInfo.IsResolved(boundNode.ReturnType)) {
    //        return null;
    //    }
    //
    //    var returnType = Visit(node.Body);
    //
    //    if (returnType != null) {
    //        boundNode.ReturnType = returnType;
    //        boundNode.Symbol.Type = returnType;
    //    }
    //    else {
    //        boundNode.ReturnType = TypeInfo.UnresolvedType;
    //        boundNode.Symbol.Type = TypeInfo.UnresolvedType;
    //    }
    //
    //    return boundNode.ReturnType;
    //}

    public override RetInfo? Visit (BlockStatement node) {
        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundBlockStatement>(node);
        // Start with unresolved type.
        List<TypeSymbol> foundRetTypes = new();

        // We'll find "return" or "yield" statements inside the block, recursively,
        // and build a list with all return types found.
        foreach (var stmt in node.Nodes) {
            if (stmt.Kind != SyntaxKind.ReturnStatement) continue;

            var retInfo = Visit(stmt);
            // If we didn't visit return / yield, retInfo will be null.
            if (retInfo.HasValue == false) continue;

            // We cannot conclude this blocks's type until every returned type
            // in it is resolved; so we just set its type to unresolved and
            // abort the procedure.
            if (retInfo.Value.type == _cmp.Native.Types.Unresolved) {
                boundNode.EvaluationKind = BlockEvaluationKind.Return;
                boundNode.Type = _cmp.Native.Types.Unresolved;
                return null;
            }

            foundRetTypes.Add(retInfo.Value.type);
        }

        // Calculate evaluation kind.
        boundNode.EvaluationKind = BlockEvaluationKind.Return; // TODO: Implement yield.

        // Calculate return type.
        if (foundRetTypes.Count == 0) {
            boundNode.Type = _cmp.Native.Types.Void;
        }
        else {
            HashSet<TypeSymbol> types = [.. foundRetTypes];

            if (types.Contains(_cmp.Native.Types.Void) && types.Count > 1) {
                Messages.Add(CompilerMessage.Analyzers.InconsistentReturnBehavior(node.Line));
                boundNode.Type = _cmp.Native.Types.Error;
            }
            // If we have more than one type, that would form a union, but
            // right now that's not implemented so we throw instead.
            else if (types.Count > 1) {
                throw new NotImplementedException(
                    "Multiple return types not implemented yet."
                );
            }
            // Else, the only type in the set is the return type.
            else {
                boundNode.Type = types.ToArray()[0];
            }
        }

        // BlockStatement is the one that keeps this information. When a function
        // or a need statement need to infer their type from their body, they
        // pull the info from this node's bound node.
        return null; 
    }

    public override RetInfo? Visit (ReturnStatement node) {
        if (node.Expression == null) {
            return (SyntaxKind.ReturnStatement, _cmp.Native.Types.Void);
        }

        var boundExpr = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);
        return (SyntaxKind.ReturnStatement, boundExpr.Type ?? _cmp.Native.Types.Unresolved);
    }
}
