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

    private readonly Compilation _cmp;
    private readonly ScopeResolver _scope;

    private Binder Binder => _cmp.Binder;
    private NativeCompilation.TypeCollection NativeTypes => _cmp.Native.Types;

    public TypeAnalyzer (Compilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp.Binder, _cmp.SymbolTable);
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

        if (boundLeft.Type != boundRight.Type) {
            Messages.Add(CompilerMessage.Analyzers.CannotAssignType(
                boundLeft.Type, boundRight.Type, node.Line)
            );
        }
    }
}
