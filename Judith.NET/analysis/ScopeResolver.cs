using Judith.NET.analysis.binder;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Judith.NET.analysis;
public class ScopeResolver {
    private Binder _binder { get; set; }
    public SymbolTable Current { get; set; }

    public ScopeResolver (Binder binder, SymbolTable root) {
        _binder = binder;
        Current = root;
    }

    /// <summary>
    /// Enters the scope given.
    /// </summary>
    public void BeginScope (SymbolTable scope) {
        Current = scope;
    }

    /// <summary>
    /// Ends the current scope and returns to the outer scope. Throws if the
    /// current scope doesn't have an outer scope.
    /// </summary>
    public void EndScope () {
        if (Current.OuterTable == null) {
            throw new($"SymbolTable '{Current.Qualifier}' should have a parent.");
        }

        Current = Current.OuterTable;
    }

    public void BeginScope (FunctionDefinition funcDef) {
        var boundFuncDef = GetBoundNodeOrThrow<BoundFunctionDefinition>(funcDef);

        Current = boundFuncDef.Scope;
    }

    public void BeginScope (StructTypeDefinition node) {
        var boundNode = GetBoundNodeOrThrow<BoundStructTypeDefinition>(node);

        Current = boundNode.Scope;
    }

    public void BeginThenScope (IfExpression ifExpr) {
        var boundIfExpr = GetBoundNodeOrThrow<BoundIfExpression>(ifExpr);

        Current = boundIfExpr.ConsequentScope;
    }

    public void BeginElseScope (IfExpression ifExpr) {
        var boundIfExpr = GetBoundNodeOrThrow<BoundIfExpression>(ifExpr);

        if (boundIfExpr.AlternateScope == null) {
            throw new("Tried to enter the 'else' scope of an if without an else!");
        }

        Current = boundIfExpr.AlternateScope;
    }

    /// <summary>
    /// Enters the scope defined by the while expression given.
    /// </summary>
    /// <param name="whileExpr">The originating node.</param>
    public void BeginScope (WhileExpression whileExpr) {
        var boundWhileExpr = GetBoundNodeOrThrow<BoundWhileExpression>(whileExpr);

        Current = boundWhileExpr.BodyScope;
    }

    private T GetBoundNodeOrThrow<T> (SyntaxNode node) where T : BoundNode {
        if (_binder.TryGetBoundNode(node, out T? boundNode) == false) {
            throw new($"Node '{node}' should be bound!");
        }

        return boundNode;
    }
}
