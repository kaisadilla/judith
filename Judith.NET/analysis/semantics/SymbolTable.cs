using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics.CodeAnalysis;

namespace Judith.NET.analysis.semantics;

[JsonConverter(typeof(StringEnumConverter))]
public enum ScopeKind {
    Global,
    Module,
    Namespace,
    FunctionBlock,
    StructSpace,
    InterfaceSpace,
    ClassSpace,
    IfBlock,
    WhileBlock,
    ElseBlock,
    ObjectInitializer,
}

public class SymbolTable {
    /// <summary>
    /// The kind of node that creates the scope represented by this table.
    /// </summary>
    public ScopeKind ScopeKind { get; protected init; }

    /// <summary>
    /// The name of the table. This is usually equal to the symbol's name, but
    /// it may differ if a disambiguating name is given to it.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// The table that contains this table, if any.
    /// </summary>
    [JsonIgnore]
    public SymbolTable? Parent { get; protected init; }

    /// <summary>
    /// The symbol that represents this table, if any. This will be defined for
    /// named scopes (such as a scope created by a function), but will remain
    /// undefined for anonymous scopes (such as a scope created by an if
    /// expression).
    /// </summary>
    public Symbol? Symbol { get; }

    /// <summary>
    /// The symbols contained by this table, indexed by unqualified name.
    /// </summary>
    public Dictionary<string, Symbol> Symbols { get; } = [];

    /// <summary>
    /// The symbol tables contained inside this one, indexed by unqualified name.
    /// </summary>
    public Dictionary<string, SymbolTable> ChildTables { get; } = [];

    /// <summary>
    /// Returns true if this is the root table of a project.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Parent))]
    public bool IsRootTable => Parent == null;

    /// <summary>
    /// Returns true if this is the global scope.
    /// </summary>
    public bool IsGlobalTable => Parent == null && Name == "";

    /// <summary>
    /// Returns true if this is an anonymous scope.
    /// </summary>
    public bool IsAnonymousTable => Symbol == null;

    public string Qualifier {
        get {
            if (Symbol != null) return Symbol.FullyQualifiedName;
            else if (Parent != null) return Parent.Qualifier + "/<anonymous-scope>";
            else return "<anonymous-scope>";
        }
    }

    protected SymbolTable (
        ScopeKind scopeKind, string name, SymbolTable? parent, Symbol? symbol
    ) {
        ScopeKind = scopeKind;
        Name = name;
        Parent = parent;
        Symbol = symbol;
    }

    public static SymbolTable CreateRootTable (string assembly, string[] name) {
        if (name.Length <= 0) throw new("At least one name must be provided.");

        Symbol rootSymbol = new(SymbolKind.Module, name[0], name[0], assembly);
        SymbolTable root = new(ScopeKind.Global, name[0], null, rootSymbol);

        SymbolTable parent = root;
        for (int i = 1; i < name.Length; i++) {
            Symbol childSymbol = new(
                SymbolKind.Module, name[i], parent.Qualify(name[i]), assembly
            );
            parent.AddSymbol(childSymbol);
            SymbolTable child = parent.CreateChildTable(ScopeKind.Module, childSymbol);

            parent = child;
        }

        return root;
    }

    /// <summary>
    /// Returns the fully qualified name for an unqualified name in this table.
    /// </summary>
    /// <param name="name">An unqualified name.</param>
    public string Qualify (string name) {
        if (Name == "") return name;
        return Qualifier + "/" + name;
    }

    /// <summary>
    /// Returns true if this table contains a symbol with the name given.
    /// </summary>
    /// <param name="name">The name of the symbol.</param>
    public bool ContainsSymbol (string name) {
        return Symbols.ContainsKey(name);
    }

    /// <summary>
    /// Returns the symbol identified by the name given, if it exists.
    /// </summary>
    /// <param name="name">The name of the symbol to search.</param>
    /// <param name="symbol">The symbol found, if any.</param>
    public bool TryGetSymbol (
        string name, [NotNullWhen(true)] out Symbol? symbol
    ) {
        return Symbols.TryGetValue(name, out symbol);
    }

    /// <summary>
    /// Returns the symbol table identified by the unqualified name given, if
    /// it exists.
    /// </summary>
    /// <param name="name">The unqualified name of the symbol.</param>
    /// <param name="symbolTable">The symbol table found, if any.</param>
    public bool TryGetChildTable (
        string name, [NotNullWhen(true)] out SymbolTable? symbolTable
    ) {
        return ChildTables.TryGetValue(name, out symbolTable);
    }

    /// <summary>
    /// Creates the symbol defined by the definer inside this table.
    /// </summary>
    /// <typeparam name="T">The type of the symbol to add.</typeparam>
    /// <param name="createSymbol">The definer function for this symbol.</param>
    /// <returns></returns>
    public void AddSymbol<T> (T symbol) where T : Symbol {
        if (Symbols.ContainsKey(symbol.Name)) throw new Exception(
            $"'{symbol.Name}' is already defined in this table."
        );

        Symbols[symbol.Name] = symbol;
    }

    /// <summary>
    /// Creates a child table of the kind given. If the symbol is null, an
    /// anonymous table will be created. If the symbol's name already exist,
    /// a disambiguating name will be created for it.
    /// </summary>
    /// <param name="kind">The kind of scope to create.</param>
    /// <param name="symbol">The symbol linked to this scope, if any. Anonymous
    /// scopes (such as the scope created in a while statement) do not have
    /// any symbols.</param>
    /// <returns></returns>
    public SymbolTable CreateChildTable (ScopeKind kind, Symbol? symbol) {
        // The name under which the scope will be indexed.
        string name = GetAvailableName(symbol?.Name ?? "<anonymous-scope>");

        SymbolTable tbl = new(kind, name, this, symbol);

        ChildTables[name] = tbl;
        return tbl;
    }

    private string GetAvailableName (string originalName) {
        string name = originalName;

        // The name may already exist. This is the case, for example, for function
        // overloads, where multiple scopes for multiple functions with the same
        // name exists. In this case, we append "`" followed by a number at the
        // end, for example "println`6".
        for (int i = 0; i <= int.MaxValue; i++) {
            if (
                ChildTables.ContainsKey(name) == false
                && Symbols.ContainsKey(name) == false
            ) {
                return name;
            }

            name = originalName + $"`{i}";

            if (i == int.MaxValue) throw new("Too many symbols with the same name!");
        }

        return name;
    }

    /// <summary>
    /// Returns the root module that contains this table.
    /// Throws an exception if the max depth is reached.
    /// </summary>
    public SymbolTable GetRootTable (int maxDepth = 256) {
        SymbolTable root = this;

        for (int i = 0; i <= maxDepth; i++) {
            if (i == maxDepth) throw new ModuleDepthTooHighException(maxDepth);

            if (root.IsRootTable) break;

            root = root.Parent;
        }

        return root;
    }

    /// <summary>
    /// Returns the table located at the path given, if it exists, taking this
    /// table as the root. Returns null otherwise.
    /// </summary>
    /// <param name="path">The path to the table sought.</param>
    public SymbolTable? GetTableInTree (string[] path) {
        if (path.Length == 0) return this;

        SymbolTable? parent = this;
        for (int i = 0; i < path.Length; i++) {
            if (parent.TryGetChildTable(path[i], out parent) == false) {
                return null;
            }
        }

        return parent;
    }
}

public class ModuleDepthTooHighException : Exception {
    public ModuleDepthTooHighException (int max)
        : base($"Maximum depth of {max} has been exceeded.")
    { }
}
