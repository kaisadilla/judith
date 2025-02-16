using Judith.NET.analysis.serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Judith.NET.analysis;

[JsonConverter(typeof(SymbolTableJsonConverter))]
public class SymbolTable {
    /// <summary>
    /// The table that contains this table, if any.
    /// </summary>
    public SymbolTable? OuterTable { get; private set; }
    /// <summary>
    /// The symbol that represents this table.
    /// </summary>
    public Symbol TableSymbol;
    /// <summary>
    /// The symbol tables that are children to this one. For example, the symbol
    /// table for "std::collections" is a child of the symbol table for "std".
    /// The keys in the dictionary are unqualified names - i.e. the table for
    /// "std::collections" is indexed by the key "collections".
    /// </summary>
    public Dictionary<string, SymbolTable> InnerTables { get; private set; } = new();
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
    /// Returns true if this is the global table.
    /// </summary>
    public bool IsGlobalTable => OuterTable == null;

    public SymbolTable (SymbolTable? outerTable, Symbol tableSymbol) {
        OuterTable = outerTable;
        TableSymbol = tableSymbol;
    }

    /// <summary>
    /// Returns a table to be used as the global module. That table's symbol
    /// will be "global" and it won't have any parent.
    /// </summary>
    public static SymbolTable CreateGlobalTable () {
        SymbolTable tbl = new(null, null!);
        tbl.TableSymbol = new(tbl, SymbolKind.Module, "", "");

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
    /// Returns the symbol identified by the name given. The symbol is searched
    /// across all ancestors of this table, starting from the innermost table
    /// (this one).
    /// </summary>
    /// <param name="name">The unqualified name of the symbol.</param>
    /// <param name="symbol">The symbol found, if any.</param>
    public bool TryGetSymbol (string name, [NotNullWhen(true)] out Symbol? symbol) {
        if (Symbols.TryGetValue(name, out symbol)) {
            return true;
        }

        if (OuterTable != null) {
            return OuterTable.TryGetSymbol(name, out symbol);
        }

        symbol = null;
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
    /// <param name="kind">The kind of symbol to create.</param>
    /// <param name="name">The unqualified name of the symbol.</param>
    public Symbol AddSymbol (SymbolKind kind, string name) {
        if (Symbols.ContainsKey(name)) {
            throw new Exception($"'{name}' is already defined in this table.");
        }

        Symbol symbol = new(this, kind, name, QualifyName(name));

        switch (kind) {
            case SymbolKind.Module:
            case SymbolKind.NativeType:
            case SymbolKind.StructType:
            case SymbolKind.InterfaceType:
            case SymbolKind.Class:
            case SymbolKind.Namespace:
            case SymbolKind.MemberFunction:
            case SymbolKind.Function:
                CreateInnerTable(kind, name);
                break;
            case SymbolKind.AliasType:
            case SymbolKind.UnionType:
            case SymbolKind.SetType:
            case SymbolKind.MemberField:
            case SymbolKind.Local:
            default:
                break;
        }


        Symbols[name] = symbol;
        return symbol;
    }

    public void CreateInnerTable (SymbolKind kind, string name) {
        SymbolTable tbl = new(this, null!);
        tbl.TableSymbol = new(tbl, kind, name, QualifyName(name));

        InnerTables[name] = tbl;
    }

    /// <summary>
    /// Returns the fully qualified name for an unqualified name in this table.
    /// </summary>
    /// <param name="name">An unqualified name.</param>
    public string QualifyName (string name) {
        if (IsGlobalTable) return name;
        else return TableSymbol.FullyQualifiedName + "/" + name;
    }
}
