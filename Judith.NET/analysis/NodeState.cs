using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

public enum NodeState {
    /// <summary>
    /// This node has never been analyzed.
    /// </summary>
    Unvisited,
    /// <summary>
    /// This node has been analyzed, but couldn't be fully resolved.
    /// </summary>
    Unresolved,
    /// <summary>
    /// This node is fully resolved.
    /// </summary>
    Completed,
}

public class NodeStateManager {
    private Dictionary<SyntaxNode, NodeState> _states = [];
    private Dictionary<SyntaxNode, SymbolTable> _scopes = [];

    public bool ResolutionMade { get; set; } = false;

    public bool IsComplete (SyntaxNode node) {
        return _states.TryGetValue(node, out var state)
            && state == NodeState.Completed;
    }

    /// <summary>
    /// Returns true if all the elements in the collection given are complete.
    /// </summary>
    /// <param name="nodes">A collection of nodes to check.</param>
    /// <returns></returns>
    public bool IsComplete (IEnumerable<SyntaxNode> nodes) {
        foreach (var node in nodes) {
            if (IsComplete(node) == false) return false;
        }

        return true;
    }

    public void Mark (SyntaxNode node, NodeState state, SymbolTable scope) {
        _states[node] = state;
        _scopes[node] = scope;
    }

    /// <summary>
    /// Marks the node as Completed if true, or Unresolved if false.
    /// </summary>
    /// <param name="node">The node to mark.</param>
    /// <param name="isResolved">Whether it's completed or unresolved.</param>
    /// <param name="workDone">Whether some work was done with the node, even
    /// if the work isn't complete.</param>
    public void Mark (SyntaxNode node, bool isResolved, SymbolTable scope, bool workDone) {
        Mark(node, isResolved ? NodeState.Completed : NodeState.Unresolved, scope);
        ResolutionMade = ResolutionMade || workDone;
    }

    public bool TryGetScope (
        SyntaxNode node, [NotNullWhen(true)] out SymbolTable? scope
    ) {
        return _scopes.TryGetValue(node, out scope);
    }

    public bool AreAllComplete () {
        return _states.Values.All(s => s == NodeState.Completed);
    }

    public List<SyntaxNode> GetIncompleteNodes () {
        return _states
            .Where(kv => kv.Value != NodeState.Completed)
            .Select(kv => kv.Key)
            .ToList();
    }
}