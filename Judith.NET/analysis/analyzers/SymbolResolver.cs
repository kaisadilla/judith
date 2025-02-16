using Judith.NET.analysis.syntax;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.analyzers;

/// <summary>
/// Traverses all the nodes in a compilation unit, resolving every indentifier
/// to a symbol in the program's symbol table.
/// </summary>
public class SymbolResolver : SyntaxVisitor {
    public MessageContainer Messages { get; private set; } = new();

    private Compilation _program;

    private SymbolTable _currentTable;

    public SymbolResolver (Compilation program) {
        _program = program;

        _currentTable = program.SymbolTable;
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    public override void Visit (FunctionDefinition node) {
        string name = node.Identifier.Name;

        if (_currentTable.TryFindSymbol(name, out Symbol? symbol) == false) {
            throw new Exception(
                $"Symbol '{name}' in " +
                $"'{_currentTable.TableSymbol.FullyQualifiedName}' should exist."
            );
        }

        node.Identifier.SetSymbol(symbol);

        if (_currentTable.TryGetInnerTable(name, out var innerTable) == false) {
            throw ExNameShouldExist(name);
        }

        _currentTable = innerTable;
        Visit(node.Parameters);
        Visit(node.Body);
    }

    public override void Visit (IdentifierExpression node) {
        string name = node.Identifier.Name;

        if (_currentTable.TryFindSymbolRecursively(name, out Symbol? symbol) == false) {
            Messages.Add(CompilerMessage.Analyzers.NameDoesNotExist(
                name, node.Identifier.Line
            ));
            return;
        }

        node.Identifier.SetSymbol(symbol);
    }

    public override void Visit (LocalDeclarator node) {
        string name = node.Identifier.Name;

        if (_currentTable.TryFindSymbol(name, out Symbol? symbol) == false) {
            throw ExNameShouldExist(name);
        }

        node.Identifier.SetSymbol(symbol);
    }

    private Exception ExNameShouldExist (string name) {
        return new Exception(
            $"Inner table '{name}' in " +
            $"'{_currentTable.TableSymbol.FullyQualifiedName}' should exist."
        );
    }
}
