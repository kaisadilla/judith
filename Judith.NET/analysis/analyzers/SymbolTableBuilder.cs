using Judith.NET.analysis.binder;
using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.analyzers;

/// <summary>
/// Traverses all the nodes in a compilation unit, creating all the symbols
/// defined in it.
/// </summary>
public class SymbolTableBuilder : SyntaxVisitor {

    private Compilation _cmp;

    private ScopeResolver _scope;

    private Dictionary<SyntaxNode, NodeState> _nodeStates = [];
    private int _resolutions = 0;

    public SymbolTableBuilder (Compilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp.Binder, _cmp.SymbolTable);
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    public override void Visit (FunctionDefinition node) {
        string name = node.Identifier.Name;

        var paramTypes = _cmp.Binder.GetParamTypes(node.Parameters);

        var (symbol, overload) = _scope.Current.AddFunctionSymbol(
            name, _cmp.Native.Types.UnresolvedFunction, paramTypes
        );
        var scope = _scope.Current.CreateInnerTable(
            ScopeKind.FunctionBlock, symbol, overload
        );

        _scope.BeginScope(scope);
        Visit(node.Parameters);
        Visit(node.Body);
        _scope.EndScope(); // If this fails, something is wrong in CreateAnonymousInnerTable().

        _cmp.Binder.BindFunctionDefinition(node, symbol, overload, scope);

        _nodeStates[node] = NodeState.Completed;
    }

    public override void Visit (StructTypeDefinition node) {
        string name = node.Identifier.Name;

        TypeSymbol symbol = _scope.Current.AddSymbol(
            TypeSymbol.Define(SymbolKind.StructType, name)
        );
        symbol.Type = _cmp.Native.Types.NoType;

        var scope = _scope.Current.CreateInnerTable(ScopeKind.StructSpace, symbol);
        var boundNode = _cmp.Binder.BindStructTypeDefinition(node, symbol, scope);

        _scope.BeginScope(scope);
        VisitMembers(boundNode, node.MemberFields);
        _scope.EndScope();

        _nodeStates[node] = NodeState.Completed;
    }

    public override void Visit (BlockStatement node) {
        // Block statements do not create symbols, but we bind them in this step anyway.
        // Block statements will use bound nodes to store their return / yield type(s).
        _cmp.Binder.BindBlockStatement(node);

        base.Visit(node);

        _nodeStates[node] = NodeState.Completed;
    }

    public override void Visit (IfExpression node) {
        Visit(node.Test);

        var consequentScope = _scope.Current.CreateAnonymousInnerTable(ScopeKind.IfBlock);
        SymbolTable? alternateScope = null;

        if (node.Alternate != null) {
            alternateScope = _scope.Current.CreateAnonymousInnerTable(ScopeKind.ElseBlock);
        }

        _cmp.Binder.BindIfExpression(node, consequentScope, alternateScope);

        _scope.BeginScope(consequentScope);
        Visit(node.Consequent);

        if (node.Alternate != null) {
            _scope.BeginScope(alternateScope!);
            Visit(node.Alternate);
        }

        _scope.EndScope();

        _nodeStates[node] = NodeState.Completed;
    }

    public override void Visit (WhileExpression node) {
        Visit(node.Test);

        var bodyScope = _scope.Current.CreateAnonymousInnerTable(ScopeKind.WhileBlock);
        
        _cmp.Binder.BindWhileExpression(node, bodyScope);

        _scope.BeginScope(bodyScope);
        Visit(node.Body);
        _scope.EndScope();

        _nodeStates[node] = NodeState.Completed;
    }

    public override void Visit (LocalDeclarator node) {
        var symbol = _scope.Current.AddSymbol(
            Symbol.Define(SymbolKind.Local, node.Identifier.Name)
        );

        _cmp.Binder.BindLocalDeclarator(node, symbol);

        _nodeStates[node] = NodeState.Completed;
    }

    public override void Visit (Parameter node) {
        var symbol = _scope.Current.AddSymbol(
            Symbol.Define(SymbolKind.Parameter, node.Declarator.Identifier.Name)
        );

        _cmp.Binder.BindParameter(node, symbol);

        _nodeStates[node] = NodeState.Completed;
    }

    public override void Visit (ObjectInitializer node) {
        var bodyScope = _scope.Current.CreateAnonymousInnerTable(
            ScopeKind.ObjectInitializer
        ); // TODO: Maybe we don't need it.

        _cmp.Binder.BindObjectInitializer(node, bodyScope);

        _nodeStates[node] = NodeState.Completed;
    }

    /// <summary>
    /// Visits the member fields inside a member container (structs, interfaces,
    /// classes...); creating a symbol for it and appending it to the container's
    /// list of members.
    /// </summary>
    /// <param name="memberContainer">The node that contains members.</param>
    /// <param name="nodes">The members contained.</param>
    private void VisitMembers (
        IBoundMemberContainer memberContainer, List<MemberField> nodes
    ) {
        foreach (var node in nodes) {
            var symbol = _scope.Current.AddSymbol(
                Symbol.Define(SymbolKind.MemberField, node.Identifier.Name)
            );

            _cmp.Binder.BindMemberField(node, symbol);
            memberContainer.AddMember(new(symbol));

            _nodeStates[node] = NodeState.Completed;
        }
    }
}
