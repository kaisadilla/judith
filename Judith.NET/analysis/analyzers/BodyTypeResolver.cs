using Judith.NET.analysis.binder;
using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Judith.NET.analysis.analyzers;

public class BodyTypeResolver : SyntaxVisitor {
    public MessageContainer Messages { get; private set; } = new();

    private JudithCompilation _cmp;
    private ScopeResolver _scope;

    // When we enter a body that accepts return values, we create a new "return"
    // context. The same happens when we enter a body that accepts "yield" values.
    // When that body ends, it consumes its relevant context, removing it from
    // the stack.
    private Stack<HashSet<TypeSymbol>> _returnContext = [];
    private Stack<HashSet<TypeSymbol>> _yieldContext = [];

    public NodeStateManager NodeStates { get; private set; } = new();

    public BodyTypeResolver (JudithCompilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp);
    }

    public void StartAnalysis (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    public void ContinueAnalysis () {
        NodeStates.ResolutionMade = false;
        var incompleteNodes = NodeStates.GetIncompleteNodes();

        foreach (var node in incompleteNodes) {
            if (NodeStates.TryGetScope(node, out var scope) == false) throw new(
                "Incomplete node should have a scope linked to it."
            );

            _scope.BeginScope(scope);
            Visit(node);
            // Do not end scope, we may be in the global scope.
        }
    }

    public override void Visit (FunctionDefinition node) {
        _returnContext.Push([]);
        Visit(node.Body);
        var retCtx = _returnContext.Pop().ToArray();

        var boundBody = _cmp.Binder.GetBoundNodeOrThrow<BoundBody>(node.Body);
        boundBody.Type = GetContextType(retCtx);

        var isResolved = TypeSymbol.IsResolved(boundBody.Type);
        NodeStates.Mark(node, isResolved, _scope.Current, isResolved);
    }

    public override void Visit (IfExpression node) {
        _yieldContext.Push([]);
        Visit(node.Consequent);
        var consequentYieldCtx = _yieldContext.Pop().ToArray();

        var boundConsequent = _cmp.Binder.GetBoundNodeOrThrow<BoundBody>(node.Consequent);
        boundConsequent.Type = GetContextType(consequentYieldCtx);

        var isResolved = TypeSymbol.IsResolved(boundConsequent.Type);

        if (node.Alternate != null) {
            _yieldContext.Push([]);
            Visit(node.Alternate);
            var alternateYieldCtx = _yieldContext.Pop().ToArray();

            var boundAlternate = _cmp.Binder.GetBoundNodeOrThrow<BoundBody>(node.Alternate);
            boundAlternate.Type = GetContextType(alternateYieldCtx);

            isResolved = isResolved && TypeSymbol.IsResolved(boundAlternate.Type);
        }

        NodeStates.Mark(node, isResolved, _scope.Current, isResolved);
    }

    public override void Visit (WhileExpression node) {
        _yieldContext.Push([]);
        Visit(node.Body);
        var yieldCtx = _yieldContext.Pop().ToArray();

        var boundBody = _cmp.Binder.GetBoundNodeOrThrow<BoundBody>(node.Body);
        boundBody.Type = GetContextType(yieldCtx);

        var isResolved = TypeSymbol.IsResolved(boundBody.Type);
        NodeStates.Mark(node, isResolved, _scope.Current, isResolved);
    }

    public override void Visit (LoopExpression node) {
        _yieldContext.Push([]);
        Visit(node.Body);
        var yieldCtx = _yieldContext.Pop().ToArray();

        var boundBody = _cmp.Binder.GetBoundNodeOrThrow<BoundBody>(node.Body);
        boundBody.Type = GetContextType(yieldCtx);

        var isResolved = TypeSymbol.IsResolved(boundBody.Type);
        NodeStates.Mark(node, isResolved, _scope.Current, isResolved);
    }

    public override void Visit (ArrowBody node) {
        bool isResolved = AutoEvaluateExpression(node.Expression);
        NodeStates.Mark(node, isResolved, _scope.Current, isResolved);
    }

    public override void Visit (ExpressionBody node) {
        bool isResolved = AutoEvaluateExpression(node.Expression);
        NodeStates.Mark(node, isResolved, _scope.Current, isResolved);
    }

    public override void Visit (ReturnStatement node) {
        if (_returnContext.TryPeek(out HashSet<TypeSymbol>? currentCtx) == false) {
            Messages.Add(CompilerMessage.Analyzers.UnexpectedReturn(node));
            return;
        }

        if (node.Expression == null) {
            currentCtx.Add(_cmp.Program.NativeHeader.TypeRefs.Void);
            return;
        }

        var boundExpr = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);
        currentCtx.Add(boundExpr.Type ?? _cmp.PseudoTypes.Unresolved);
    }

    public override void Visit (YieldStatement node) {
        if (_yieldContext.TryPeek(out HashSet<TypeSymbol>? currentCtx) == false) {
            Messages.Add(CompilerMessage.Analyzers.UnexpectedYield(node));
            return;
        }

        var boundExpr = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(node.Expression);
        currentCtx.Add(boundExpr.Type ?? _cmp.PseudoTypes.Unresolved);
    }

    /// <summary>
    /// Given a context, returns the type it evaluates to.
    /// </summary>
    /// <param name="context">An evaluation context (return, yield, etc...).</param>
    /// <returns></returns>
    private TypeSymbol GetContextType (TypeSymbol[] context) {
        // If any of the teypes in the context is unresolved, then we can't
        // resolve the context's type.
        if (context.Any(t => TypeSymbol.IsResolved(t) == false)) {
            return _cmp.PseudoTypes.Unresolved;
        }

        // If we didn't collect any statement, then this context's type is Void.
        if (context.Length == 0) {
            return _cmp.Program.NativeHeader.TypeRefs.Void;
        }
        // If we only collected one type, that type becomes the context's type.
        else if (context.Length == 1) {
            return context[0];
        }
        // If we've collected more than one type, the context's type is the union
        // of all the types encountered.
        else {
            throw new NotImplementedException("Union types not yet implemented.");
        }
    }

    /// <summary>
    /// Interprets the expression given as an evaluating statement (yield, return,
    /// etc...) based on the current contexts; and adds its type to the relevant
    /// context. Returns whether the type of the expression was resolved.
    /// </summary>
    /// <param name="expr">The evaluated expression.</param>
    /// <returns></returns>
    private bool AutoEvaluateExpression (Expression expr) {
        // In Judith, yield contexts have precedence over return contexts. If
        // we are inside both, then this arrow body is yielding a value.
        if (_yieldContext.TryPeek(out HashSet<TypeSymbol>? currentCtx) == false) {
            // If this is not a yield context, then it should be a return context.
            if (_returnContext.TryPeek(out currentCtx) == false) {
                Messages.Add(CompilerMessage.Analyzers.UnexpectedReturn(expr));
                return true; // true since it's been correctly evaluated to "you can't do this".
            }
        }

        var boundExpr = _cmp.Binder.GetBoundNodeOrThrow<BoundExpression>(expr);
        currentCtx.Add(boundExpr.Type ?? _cmp.PseudoTypes.Unresolved);

        return TypeSymbol.IsResolved(boundExpr.Type);
    }
}
