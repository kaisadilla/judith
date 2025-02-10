using Judith.NET.syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

/// <summary>
/// This class scans syntax trees to build a table containing all the
/// identifiers defined in it.
/// </summary>
public class SymbolCollectionAnalyzer : SyntaxVisitor {
    public List<string> ExistingFunctions { get; private set; } = new();

    public void Analyze (SyntaxNode node) {
        Visit(node);
    }

    public void Analyze (IEnumerable<SyntaxNode> nodes) {
        foreach (var node in nodes) {
            Analyze(node);
        }
    }

    public override void Visit (FunctionItem node) {
        ExistingFunctions.Add(node.Identifier.Name);
        base.Visit(node);
    }
}
