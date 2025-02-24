using Judith.NET.diagnostics.serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics.CodeAnalysis;

namespace Judith.NET.analysis;

[JsonConverter(typeof(StringEnumConverter))]
public enum ScopeKind {
    Global,
    Namespace,
    FunctionBlock,
    StructSpace,
    InterfaceSpace,
    ClassSpace,
    IfBlock,
    WhileBlock,
    ElseBlock,
}

[JsonConverter(typeof(SymbolTableJsonConverter))]
public class SymbolTable {
    /// <summary>
    /// The kind of node that creates the scope represented by this table.
    /// </summary>
    public ScopeKind ScopeKind { get; private init; }
    /// <summary>
    /// The table that contains this table, if any.
    /// </summary>
    public SymbolTable? OuterTable { get; private set; }
    /// <summary>
    /// The symbol that represents this table, if any. This will be defined for
    /// named scopes (such as a scope created by a function), but will remain
    /// undefined for anonymous scopes (such as a scope created by an if
    /// expression).
    /// </summary>
    public Symbol? TableSymbol;
    /// <summary>
    /// The symbol tables that are children to this one. For example, the symbol
    /// table for "std::collections" is a child of the symbol table for "std".
    /// The keys in the dictionary are unqualified names - i.e. the table for
    /// "std::collections" is indexed by the key "collections".
    /// </summary>
    public Dictionary<string, SymbolTable> InnerTables { get; private set; } = new();
    /// <summary>
    /// Symbol tables created by anonymous scopes, such as the body of an "if"
    /// expression.
    /// </summary>
    public List<SymbolTable> AnonymousInnerTables { get; private set; } = new();
    /// <summary>
    /// The symbols contained directly in this table (i.e. not contained in
    /// children tables). This collection also includes the Symbols that define
    /// its inner tables (e.g. if this symbol table is for module "std",
    /// this table contains the symbol for "std::collections", but not the
    /// symbols defined inside that inner module). This collection does not
    /// include the symbol that represents this table.
    /// </summary>
    public Dictionary<string, Symbol> Symbols { get; private set; } = new();

    /// <summary>
    /// The function symbols contained directly in this table. Each key points
    /// to a list of function symbols that have the same identifier, but
    /// different overloads.
    /// </summary>
    public Dictionary<string, List<FunctionSymbol>> FunctionSymbols { get; private set; } = new();

    /// <summary>
    /// Returns true if this is the global table.
    /// </summary>
    [JsonIgnore]
    public bool IsGlobalTable => OuterTable == null;

    [JsonIgnore]
    public string Qualifier {
        get {
            if (TableSymbol != null) return TableSymbol.FullyQualifiedName;
            else if (OuterTable != null) return OuterTable.Qualifier + "/<anonymous-scope>";
            else return "<anonymous-scope>";
        }
    }

    public SymbolTable (ScopeKind scopeKind, SymbolTable? outerTable, Symbol? tableSymbol) {
        ScopeKind = scopeKind;
        OuterTable = outerTable;
        TableSymbol = tableSymbol;
    }

    /// <summary>
    /// Returns a table to be used as the global module. That table's symbol
    /// will be "global" and it won't have any parent.
    /// </summary>
    public static SymbolTable CreateGlobalTable () {
        SymbolTable tbl = new(ScopeKind.Global, null, null);
        tbl.TableSymbol = new(tbl, SymbolKind.Module, "", "");

        return tbl;
    }

    public SymbolTable CreateInnerTable (ScopeKind scopeKind, Symbol symbol) {
        SymbolTable tbl = new(scopeKind, this, null);
        tbl.TableSymbol = symbol;

        InnerTables[symbol.Name] = tbl;
        return tbl;
    }

    public SymbolTable CreateAnonymousInnerTable (ScopeKind scopeKind) {
        SymbolTable tbl = new(scopeKind, this, null);

        AnonymousInnerTables.Add(tbl);
        return tbl;
    }

    /// <summary>
    /// Returns true if this table contains a symbol with the name given. Note
    /// that this search isn't recursive: if the symbol doesn't exist in this
    /// table, but exists in an outer table, this method will still return false.
    /// A symbol can be added to this table safely if this method returns false.
    /// </summary>
    /// <param name="name">The unqualified name of the symbol.</param>
    public bool ContainsSymbol (string name) {
        return Symbols.ContainsKey(name);
    }

    /// <summary>
    /// Returns the symbol identified by the name given. The symbol is only
    /// searched inside this table, not any of its ancestors, so this symbol
    /// must be defined specifically in this scope.
    /// </summary>
    /// <param name="name">The unqualified name of the symbol.</param>
    /// <param name="symbol">The symbol found, if any.</param>
    /// <returns></returns>
    public bool TryFindSymbol (string name, [NotNullWhen(true)] out Symbol? symbol) {
        return Symbols.TryGetValue(name, out symbol);
    }

    /// <summary>
    /// Returns the symbol identified by the name given. The symbol is searched
    /// across all ancestors of this table, starting from the innermost table
    /// (this one).
    /// </summary>
    /// <param name="name">The unqualified name of the symbol.</param>
    /// <param name="symbol">The symbol found, if any.</param>
    public bool TryFindSymbolRecursively (
        string name, [NotNullWhen(true)] out Symbol? symbol
    ) {
        if (Symbols.TryGetValue(name, out symbol)) {
            return true;
        }

        if (OuterTable != null) {
            return OuterTable.TryFindSymbolRecursively(name, out symbol);
        }

        symbol = null;
        return false;
    }

    /// <summary>
    /// Returns the list of function symbols identified by the given function
    /// name. This list is searched across all ancestors of this table, starting
    /// from the innermost table (this one).
    /// </summary>
    /// <param name="name">The unqualified name of the function.</param>
    /// <param name="functionSymbols">A list of all symbols defined by that function.</param>
    /// <returns></returns>
    public bool TryFindFunctionSymbolsRecursively (
        string name, [NotNullWhen(true)] out List<FunctionSymbol>? functionSymbols
    ) {
        if (FunctionSymbols.TryGetValue(name, out functionSymbols)) {
            return true;
        }

        if (OuterTable != null) {
            return OuterTable.TryFindFunctionSymbolsRecursively(
                name, out functionSymbols
            );
        }

        functionSymbols = null;
        return false;
    }

    /// <summary>
    /// Returns the symbol table identified by the unqualified name given, if
    /// it exists.
    /// </summary>
    /// <param name="name">The unqualified name of the symbol.</param>
    /// <param name="table">The symbol table found, if any.</param>
    /// <returns></returns>
    public bool TryGetInnerTable (
        string name, [NotNullWhen(true)] out SymbolTable? table
    ) {
        return InnerTables.TryGetValue(name, out table);
    }

    /// <summary>
    /// Adds a symbol to this table. An exception will occur if a symbol with
    /// that name already exists in this table (but not its parents or children).
    /// Returns the symbol that has been created.
    /// </summary>
    /// <param name="symbolKind">The kind of symbol to create.</param>
    /// <param name="name">The unqualified name of the symbol.</param>
    public Symbol AddSymbol (SymbolKind symbolKind, string name) {
        if (Symbols.ContainsKey(name)) {
            throw new Exception($"'{name}' is already defined in this table.");
        }

        Symbol symbol = new(this, symbolKind, name, QualifyName(name));
        Symbols[name] = symbol;

        return symbol;
    }

    /// <summary>
    /// Adds a function symbol to this table. Duplicate overloads will not be
    /// checked.
    /// </summary>
    /// <param name="name">The unqualified name of the function.</param>
    /// <param name="overload">The type of each parameter, in order.</param>
    public FunctionSymbol AddFunctionSymbol (string name, List<TypeInfo> overload) {
        if (FunctionSymbols.TryGetValue(name, out var funcList) == false) {
            funcList = new();
            FunctionSymbols[name] = funcList;
        }

        FunctionSymbol funcSymbol = new(this, name, QualifyName(name), overload, null);

        // We don't check for duplicate overloads here.
        funcList.Add(funcSymbol);

        return funcSymbol;
    }

    /// <summary>
    /// Returns the fully qualified name for an unqualified name in this table.
    /// </summary>
    /// <param name="name">An unqualified name.</param>
    public string QualifyName (string name) {
        if (IsGlobalTable) return name;
        else return Qualifier + "/" + name;
    }
}
