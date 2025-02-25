using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
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

    public bool IsComplete (SyntaxNode node) {
        return _states.TryGetValue(node, out var state)
            && state == NodeState.Completed;
    }

    public void Mark (SyntaxNode node, NodeState state) {
        _states[node] = state;
    }

    public void Unresolved (SyntaxNode node) => Mark(node, NodeState.Unresolved);
    public void Completed (SyntaxNode node) => Mark(node, NodeState.Completed);
}