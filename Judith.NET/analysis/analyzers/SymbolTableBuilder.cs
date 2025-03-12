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

/// <summary>
/// Traverses all the nodes in a compilation unit, creating all the symbols
/// defined in it.
/// </summary>
public class SymbolTableBuilder : SyntaxVisitor {
    public MessageContainer Messages { get; private set; } = new();

    private Compilation _cmp;

    private ScopeResolver _scope;
    private SymbolFinder _finder;

    private Dictionary<SyntaxNode, NodeState> _nodeStates = [];
    private int _resolutions = 0;

    public SymbolTableBuilder (Compilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp);
        _finder = new(_cmp);
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    public override void Visit (FunctionDefinition node) {
        string name = node.Identifier.Name;

        if (_finder.IsSymbolDefinedInScope(name, _scope.Current)) {
            Messages.Add(CompilerMessage.Analyzers.DefinitionAlreadyExist(
                name, node.Line
            ));
            return;
        }

        var paramTypes = _cmp.Binder.GetParamTypes(node.Parameters);

        var symbol = _scope.Current.AddSymbol(tbl => new FunctionSymbol(
            paramTypes, name, tbl.Qualify(name), _cmp.Name
        ));
        symbol.Type = _cmp.Native.Types.Function;
        var scope = _scope.Current.CreateChildTable(ScopeKind.FunctionBlock, symbol);
        _scope.BeginScope(scope);
        Visit(node.Parameters);
        Visit(node.Body);
        _scope.EndScope(); // If this fails, something is wrong in CreateChildTable().

        _cmp.Binder.BindFunctionDefinition(node, symbol, scope);

        _nodeStates[node] = NodeState.Completed;
    }

    public override void Visit (StructTypeDefinition node) {
        string name = node.Identifier.Name;

        if (_finder.IsSymbolDefinedInScope(name, _scope.Current)) {
            Messages.Add(CompilerMessage.Analyzers.DefinitionAlreadyExist(
                name, node.Line
            ));
            return;
        }

        TypeSymbol symbol = _scope.Current.AddSymbol(tbl => new TypeSymbol(
            SymbolKind.StructType, name, tbl.Qualify(name), _cmp.Name
        ));
        symbol.Type = _cmp.Native.Types.NoType;

        var scope = _scope.Current.CreateChildTable(ScopeKind.StructSpace, symbol);
        var boundNode = _cmp.Binder.BindStructTypeDefinition(node, symbol, scope);

        _scope.BeginScope(scope);
        VisitMembers(symbol, node.MemberFields);
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

        var consequentScope = _scope.Current.CreateChildTable(ScopeKind.IfBlock, null);
        SymbolTable? alternateScope = null;

        if (node.Alternate != null) {
            alternateScope = _scope.Current.CreateChildTable(ScopeKind.ElseBlock, null);
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

        var bodyScope = _scope.Current.CreateChildTable(ScopeKind.WhileBlock, null);
        
        _cmp.Binder.BindWhileExpression(node, bodyScope);

        _scope.BeginScope(bodyScope);
        Visit(node.Body);
        _scope.EndScope();

        _nodeStates[node] = NodeState.Completed;
    }

    public override void Visit (LocalDeclarator node) {
        var symbol = _scope.Current.AddSymbol(tbl => new Symbol(
            SymbolKind.Local, node.Identifier.Name, tbl.Qualify(node.Identifier.Name), _cmp.Name
        ));

        _cmp.Binder.BindLocalDeclarator(node, symbol);

        _nodeStates[node] = NodeState.Completed;
    }

    public override void Visit (Parameter node) {
        var symbol = _scope.Current.AddSymbol(tbl => new Symbol(
            SymbolKind.Parameter,
            node.Declarator.Identifier.Name,
            tbl.Qualify(node.Declarator.Identifier.Name),
            _cmp.Name
        ));

        _cmp.Binder.BindParameter(node, symbol);

        _nodeStates[node] = NodeState.Completed;
    }

    public override void Visit (ObjectInitializer node) {
        var bodyScope = _scope.Current.CreateChildTable(
            ScopeKind.ObjectInitializer, null
        ); // TODO: Maybe we don't need it.

        _cmp.Binder.BindObjectInitializer(node, bodyScope);

        _nodeStates[node] = NodeState.Completed;
    }

    /// <summary>
    /// Visits the member fields inside a member container (structs, interfaces,
    /// classes...); creating a symbol for it and appending it to the container's
    /// list of members.
    /// </summary>
    /// <param name="typeSymbol">The type symbol that contains members.</param>
    /// <param name="nodes">The members contained.</param>
    private void VisitMembers (
        TypeSymbol typeSymbol, List<MemberField> nodes
    ) {
        foreach (var node in nodes) {
            // TODO: Check member methods.
            var memberSymbol = _scope.Current.AddSymbol(tbl => new MemberSymbol(
                SymbolKind.MemberField,
                node.Identifier.Name,
                tbl.Qualify(node.Identifier.Name),
                _cmp.Name
            ));

            _cmp.Binder.BindMemberField(node, memberSymbol);
            typeSymbol.MemberFields.Add(memberSymbol);

            _nodeStates[node] = NodeState.Completed;
        }
    }
}
