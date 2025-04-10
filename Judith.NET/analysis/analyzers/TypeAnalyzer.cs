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

public class TypeAnalyzer : SyntaxVisitor {
    public MessageContainer Messages { get; private set; } = new();

    private readonly JudithCompilation _cmp;
    private readonly ScopeResolver _scope;

    private Binder Binder => _cmp.Binder;

    public TypeAnalyzer (JudithCompilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp);
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    public override void Visit (AssignmentExpression node) {
        // Check num type coalescing
        var boundLeft = Binder.GetBoundNodeOrThrow<BoundExpression>(node.Left);
        var boundRight = Binder.GetBoundNodeOrThrow<BoundExpression>(node.Right);

        if (
            TypeSymbol.IsResolved(boundLeft.Type) == false 
            || TypeSymbol.IsResolved(boundRight.Type) == false
        ) {
            throw new("Types are not resolved.");
        }

        if (IsAssignable(boundRight.Type, boundLeft.Type) == false) {
            Messages.Add(CompilerMessage.Analyzers.CannotAssignType(
                node, boundLeft.Type, boundRight.Type
            ));
        }
    }

    /// <summary>
    /// Returns true if the type given is assignable to a receiver of the type
    /// given.
    /// </summary>
    /// <param name="type">The type of the value to assign.</param>
    /// <param name="receiver">The type of the receiver of said value.</param>
    /// <returns></returns>
    private bool IsAssignable (TypeSymbol type, TypeSymbol receiver) {
        // If both are the same type, then it's always allowed.
        if (type == receiver) return true;

        // If the receiver is "Any", any type can be assigned to it.
        if (receiver == _cmp.NativeTypes.Any) return true;

        // If the type is "Any", it cannot be assigned to any receiver whose
        // type is not "Any".
        if (type == _cmp.NativeTypes.Any) return false;

        // TODO: Check compatible types.

        // If no implicit transformation exists, then it cannot be assigned.
        return false;
    }
}
