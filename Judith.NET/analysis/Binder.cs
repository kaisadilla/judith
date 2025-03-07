using Judith.NET.analysis.binder;
using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using Judith.NET.message;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Judith.NET.analysis;

public class Binder {
    [JsonIgnore]
    public MessageContainer Messages { get; private set; } = new();

    [JsonIgnore]
    private ProjectCompilation _cmp;

    public Dictionary<SyntaxNode, BoundNode> BoundNodes { get; private set; } = new();

    public Binder (ProjectCompilation compilation) {
        _cmp = compilation;
    }

    public bool TryGetBoundNode<T> (
        SyntaxNode node, [NotNullWhen(true)] out T? typedBoundNode
    ) where T : BoundNode {
        if (BoundNodes.TryGetValue(node, out BoundNode? boundNode)) {
            if (boundNode is T t) {
                typedBoundNode = t;
                return true;
            }
            else throw new Exception(
                $"Bound node for node of type '{node.GetType().Name}' is " +
                $"not of the expected type."
            );
        }
        else {
            typedBoundNode = null;
            return false;
        }
    }

    public T GetBoundNodeOrThrow<T> (SyntaxNode node) where T : BoundNode {
        if (TryGetBoundNode(node, out T? typedBoundNode) == false) {
            throw new($"'{node}' should be bound.");
        }

        return typedBoundNode;
    }

    public IBoundIdentifyingExpression GetBoundIdOrThrow (SyntaxNode node) {
        if (TryGetBoundNode(node, out BoundExpression? typedBoundNode) == false) {
            throw new($"'{node}' should be bound.");
        }
        if (typedBoundNode is not IBoundIdentifyingExpression boundId) {
            throw new($"'{typedBoundNode}' is not an identifying expression.");
        }

        return boundId;
    }

    public BoundFunctionDefinition BindFunctionDefinition (
        FunctionDefinition funcDef,
        FunctionSymbol symbol,
        FunctionOverloadSymbol overload,
        SymbolTable scope
    ) {
        if (TryGetBoundNode(
            funcDef, out BoundFunctionDefinition? boundFuncDef
        ) == false) {
            boundFuncDef = new(funcDef, symbol, overload, scope);
            BoundNodes[funcDef] = boundFuncDef;
        }

        return boundFuncDef;
    }

    public BoundStructTypeDefinition BindStructTypeDefinition (
        StructTypeDefinition structTypedef, TypeSymbol symbol, SymbolTable scope
    ) {
        if (TryGetBoundNode(
            structTypedef, out BoundStructTypeDefinition? boundStructTypeDef
        ) == false) {
            boundStructTypeDef = new(structTypedef, symbol, scope);
            BoundNodes[structTypedef] = boundStructTypeDef;
        }

        return boundStructTypeDef;
    }

    public BoundBlockStatement BindBlockStatement (BlockStatement blockStmt) {
        if (TryGetBoundNode(blockStmt, out BoundBlockStatement? boundBlockStmt) == false) {
            boundBlockStmt = new(blockStmt);
            BoundNodes[blockStmt] = boundBlockStmt;
        }

        return boundBlockStmt;
    }

    public BoundLocalDeclarationStatement BindLocalDeclarationStatement (
        LocalDeclarationStatement localDeclStmt
    ) {
        if (TryGetBoundNode(
            localDeclStmt, out BoundLocalDeclarationStatement? boundLocalDeclStmt
        ) == false) {
            boundLocalDeclStmt = new(localDeclStmt);
            BoundNodes[localDeclStmt] = boundLocalDeclStmt;
        }

        return boundLocalDeclStmt;
    }

    public BoundReturnStatement BindReturnStatement (ReturnStatement returnStmt) {
        if (TryGetBoundNode(
            returnStmt, out BoundReturnStatement? boundReturnStmt) == false
        ) {
            boundReturnStmt = new(returnStmt);
            BoundNodes[returnStmt] = boundReturnStmt;
        }

        return boundReturnStmt;
    }

    public BoundYieldStatement BindYieldStatement (YieldStatement yieldStmt) {
        if (TryGetBoundNode(yieldStmt, out BoundYieldStatement? boundYieldStmt) == false) {
            boundYieldStmt = new(yieldStmt);
            BoundNodes[yieldStmt] = boundYieldStmt;
        }

        return boundYieldStmt;
    }

    public BoundIfExpression BindIfExpression (
        IfExpression ifExpr, SymbolTable consequentScope, SymbolTable? alternateScope
    ) {
        if (TryGetBoundNode(ifExpr, out BoundIfExpression? boundIfExpr)) {
            return boundIfExpr;
        }

        boundIfExpr = new(ifExpr, consequentScope, alternateScope);
        BoundNodes[ifExpr] = boundIfExpr;

        return boundIfExpr;
    }

    public BoundWhileExpression BindWhileExpression (
        WhileExpression whileExpr, SymbolTable bodyScope
    ) {
        if (TryGetBoundNode(whileExpr, out BoundWhileExpression? boundWhileExpr)) {
            return boundWhileExpr;
        }

        boundWhileExpr = new(whileExpr, bodyScope);
        BoundNodes[whileExpr] = boundWhileExpr;

        return boundWhileExpr;
    }

    public BoundAssignmentExpression BindAssignmentExpression (
        AssignmentExpression assignmentExpr
    ) {
        if (TryGetBoundNode(assignmentExpr, out BoundAssignmentExpression? boundAssignmentExpr) == false) {
            boundAssignmentExpr = new(assignmentExpr);
            BoundNodes[assignmentExpr] = boundAssignmentExpr;
        }

        return boundAssignmentExpr;
    }

    public BoundBinaryExpression BindBinaryExpression (BinaryExpression binaryExpr) {
        if (TryGetBoundNode(binaryExpr, out BoundBinaryExpression? boundBinaryExpr) == false) {
            boundBinaryExpr = new(binaryExpr);
            BoundNodes[binaryExpr] = boundBinaryExpr;
        }

        return boundBinaryExpr;
    }

    public BoundLeftUnaryExpression BindLeftUnaryExpression (LeftUnaryExpression leftUnaryExpr) {
        if (TryGetBoundNode(leftUnaryExpr, out BoundLeftUnaryExpression? boundLeftUnaryExpr) == false) {
            boundLeftUnaryExpr = new(leftUnaryExpr);
            BoundNodes[leftUnaryExpr] = boundLeftUnaryExpr;
        }

        return boundLeftUnaryExpr;
    }

    public BoundObjectInitializationExpression BindObjectInitializationExpression (
        ObjectInitializationExpression initExpr
    ) {
        if (TryGetBoundNode(
            initExpr, out BoundObjectInitializationExpression? boundInitExpr
        ) == false) {
            boundInitExpr = new(initExpr);
            BoundNodes[initExpr] = boundInitExpr;
        }

        return boundInitExpr;
    }

    public BoundAccessExpression BindAccessExpression (
        AccessExpression accessExpr, MemberSymbol memberSymbol
    ) {
        if (TryGetBoundNode(accessExpr, out BoundAccessExpression? boundAccessExpr) == false) {
            boundAccessExpr = new(accessExpr, memberSymbol);
            BoundNodes[accessExpr] = boundAccessExpr;
        }

        return boundAccessExpr;
    }

    public BoundCallExpression BindCallExpression (CallExpression callExpr) {
        if (TryGetBoundNode(callExpr, out BoundCallExpression? boundCallExpression) == false) {
            boundCallExpression = new(callExpr);
            BoundNodes[callExpr] = boundCallExpression;
        }

        return boundCallExpression;
    }

    public BoundGroupExpression BindGroupExpression (GroupExpression groupExpr) {
        if (TryGetBoundNode(groupExpr, out BoundGroupExpression? boundGroupExpr) == false) {
            boundGroupExpr = new(groupExpr);
            BoundNodes[groupExpr] = boundGroupExpr;
        }

        return boundGroupExpr;
    }

    public BoundIdentifierExpression BindIdentifierExpression (
        IdentifierExpression idExpr, Symbol symbol
    ) {
        if (TryGetBoundNode(idExpr, out BoundIdentifierExpression? boundIdExpr) == false) {
            boundIdExpr = new(idExpr, symbol);
            BoundNodes[idExpr] = boundIdExpr;
        }

        return boundIdExpr;
    }

    public BoundLiteralExpression BindLiteralExpression (LiteralExpression litExpr) {
        if (TryGetBoundNode(litExpr, out BoundLiteralExpression? boundLitExpr)) {
            return boundLitExpr;
        }

        switch (litExpr.Literal.TokenKind) {
            case TokenKind.KwTrue:
            case TokenKind.KwFalse:
                return ResolveBooleanLiteralExpression(litExpr);
            case TokenKind.Number:
                return ResolveNumberLiteralExpression(litExpr);
            case TokenKind.String:
                return ResolveStringLiteralExpression(litExpr);
            default:
                throw new Exception(
                    $"Token kind '{litExpr.Literal.TokenKind}' is not valid for a " +
                    $"literal."
                );
        }
    }

    public BoundLocalDeclarator BindLocalDeclarator (
        LocalDeclarator localDecl, Symbol symbol
    ) {
        if (TryGetBoundNode(localDecl, out BoundLocalDeclarator? boundLocalDecl) == false) { 
            boundLocalDecl = new(localDecl, symbol);
            BoundNodes[localDecl] = boundLocalDecl;
        }

        // The type of a local declarator is calculated by the node that contains it
        // (e.g. by the LocalDeclarationStatement that contains them).

        return boundLocalDecl;
    }

    public BoundTypeAnnotation BindTypeAnnotation (
        TypeAnnotation typeAnnt, TypeSymbol type
    ) {
        if (TryGetBoundNode(typeAnnt, out BoundTypeAnnotation? boundTypeAnnt) == false) {
            boundTypeAnnt = new(typeAnnt, type);
            BoundNodes[typeAnnt] = boundTypeAnnt;
        }

        return boundTypeAnnt;
    }

    public BoundParameter BindParameter (Parameter param, Symbol symbol) {
        if (TryGetBoundNode(param, out BoundParameter? boundParam) == false) {
            boundParam = new(param, symbol);
            BoundNodes[param] = boundParam;
        }

        return boundParam;
    }

    public BoundObjectInitializer BindObjectInitializer (
        ObjectInitializer init, SymbolTable scope
    ) {
        if (TryGetBoundNode(init, out BoundObjectInitializer? boundInit) == false) {
            boundInit = new(init, scope);
            BoundNodes[init] = boundInit;
        }

        return boundInit;
    }

    public BoundMemberField BindMemberField (MemberField field, Symbol symbol) {
        if (TryGetBoundNode(field, out BoundMemberField? boundMemberField) == false) {
            boundMemberField = new(field, symbol);
            BoundNodes[field] = boundMemberField;
        }

        return boundMemberField;
    }

    #region Resolve literals
    private BoundLiteralExpression ResolveBooleanLiteralExpression (LiteralExpression expr) {
        BoundLiteralExpression bound;
        if (expr.Literal.TokenKind == TokenKind.KwTrue) {
            bound = new(expr, _cmp.Native.Types.Bool, new ConstantValue(true));
        }
        else if (expr.Literal.TokenKind ==TokenKind.KwFalse) {
            bound = new(expr, _cmp.Native.Types.Bool, new ConstantValue(false));
        }
        else throw new Exception(
            $"Token kind '{expr.Literal.TokenKind}' is not valid for a boolean " +
            $"literal"
        );

        BoundNodes[expr] = bound;
        return bound;
    }

    private BoundLiteralExpression ResolveNumberLiteralExpression (LiteralExpression expr) {
        BoundLiteralExpression bound;

        // The part of the lexeme that represents the number, removing
        // underscores as they don't have any meaning. Prefixes and suffixes
        // will eventeually be removed from this string, too.
        string str = expr.Literal.Source.Replace("_", "");
        // If it contains a dot, it's decimal and will be parsed in a double.
        // Same applies if it's expressed in scientific notation: XeY.
        bool isDecimal = str.Contains('.') || str.Contains('e');

        string? prefix = null;
        string? suffix = null;

        // If there's a base prefix, store it in prefix and remove it from the
        // value string.
        if (str.StartsWith("0x") || str.StartsWith("0b") || str.StartsWith("0o")) {
            prefix = str[..2];
            str = str[2..];
        }

        // Detect whether we have a suffix (i64, f, ib...). If we do, store it
        // in suffix and remove it from the value string.
        int suffixIndex = -1;
        for (int i = 0; i < str.Length; i++) {
            char c = str[i];

            if (c == 'e' || c == 'E') continue;

            if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) {
                suffixIndex = i;
                break;
            }
        }
        if (suffixIndex > 0) {
            suffix = str[suffixIndex..];
            str = str[..suffixIndex];
        }

        if (suffix == "d" || suffix == "ib") {
            throw new NotImplementedException(
                "Cannot yet compile Decimal and BigInt numbers."
            );
        }

        // Decimal values get treated as doubles inside the compiler.
        if (isDecimal) {
            if (prefix != null) {
                throw new NotImplementedException(
                    "TODO: Can Judith define non-base-10 decimal values?"
                );
            }

            double value;
            if (suffix == "f64" || suffix == "f" || suffix == null) {
                try {
                    value = double.Parse(str, CultureInfo.InvariantCulture);
                }
                catch (OverflowException) {
                    Messages.Add(CompilerMessage.Analyzers.FloatLiteralOverflow(
                        str, "F64", expr.Literal.Line
                    ));
                    value = 0;
                }

                bound = new(expr, _cmp.Native.Types.Num, new((double)value)); // TODO: F64
            }
            else if (suffix == "f32") {
                try {
                    value = float.Parse(str, CultureInfo.InvariantCulture);
                }
                catch (OverflowException) {
                    Messages.Add(CompilerMessage.Analyzers.FloatLiteralOverflow(
                        str, "F32", expr.Literal.Line
                    ));
                    value = 0;
                }

                bound = new(expr, _cmp.Native.Types.F32, new((double)value));
            }
            else {
                Messages.Add(CompilerMessage.Analyzers.NumberSuffixCannotBeUsedForDecimal(
                    suffix, expr.Literal.Line
                ));
                bound = new(expr, _cmp.Native.Types.F64, new(0d));
            }

        }
        // Other values get treated as long integers.
        else {
            // Unsigned integer
            if (suffix != null && suffix[0] == 'u') {
                ulong value;
                try {
                    value = prefix switch {
                        null => Convert.ToUInt64(str, CultureInfo.InvariantCulture),
                        "0x" => Convert.ToUInt64(str, 16),
                        "0b" => Convert.ToUInt64(str, 2),
                        "0o" => Convert.ToUInt64(str, 8),
                        _ => throw new Exception("Impossible prefix found."),
                    };
                }
                catch (OverflowException) {
                    Messages.Add(CompilerMessage.Analyzers.IntegerLiteralIsTooLarge(
                        expr.Literal.Line
                    ));
                    value = 0;
                }

                // suffixes "i64", "i" and null are always ok. else:
                if (suffix == "u64" || suffix == "u") {
                    bound = new(expr, _cmp.Native.Types.Ui64, new((ulong)value));
                }
                else if (suffix == "u32") {
                    CheckUnsignedIntegerSize(value, int.MaxValue, "U32");
                    bound = new(expr, _cmp.Native.Types.Ui32, new((ulong)value));
                }
                else if (suffix == "u16") {
                    CheckUnsignedIntegerSize(value, int.MaxValue, "U16");
                    bound = new(expr, _cmp.Native.Types.Ui16, new((ulong)value));
                }
                else if (suffix == "u8") {
                    CheckUnsignedIntegerSize(value, int.MaxValue, "U8");
                    bound = new(expr, _cmp.Native.Types.Ui8, new((ulong)value));
                }
                else {
                    bound = new(expr, GetSuffixTypeInfo(suffix), new((ulong)value));
                }
            }
            // Signed integer
            else {
                long value;
                try {
                    value = prefix switch {
                        null => Convert.ToInt64(str, CultureInfo.InvariantCulture),
                        "0x" => Convert.ToInt64(str, 16),
                        "0b" => Convert.ToInt64(str, 2),
                        "0o" => Convert.ToInt64(str, 8),
                        _ => throw new Exception("Impossible prefix found."),
                    };
                }
                catch (OverflowException) {
                    Messages.Add(CompilerMessage.Analyzers.IntegerLiteralIsTooLarge(
                        expr.Literal.Line
                    ));
                    value = 0;
                }

                // suffixes "i64", "i" and null are always ok. else:
                if (suffix == "i64" || suffix == "i" || suffix == null) {
                    bound = new(expr, _cmp.Native.Types.Num, new((double)value)); // TODO: I64, long.
                }
                else if (suffix == "i32") {
                    CheckIntegerSize(value, int.MaxValue, "I32");
                    bound = new(expr, _cmp.Native.Types.I32, new((long)value));
                }
                else if (suffix == "i16") {
                    CheckIntegerSize(value, short.MaxValue, "I16");
                    bound = new(expr, _cmp.Native.Types.I16, new((long)value));
                }
                else if (suffix == "i8") {
                    CheckIntegerSize(value, short.MaxValue, "I8");
                    bound = new(expr, _cmp.Native.Types.I8, new((long)value));
                }
                else {
                    bound = new(expr, GetSuffixTypeInfo(suffix), new((long)value));
                }
            }
        }
        // TODO: Deal with Decimal (d) and BigInt (ib) types.
        BoundNodes[expr] = bound;
        return bound;

        void CheckIntegerSize (long value, long max, string type) {
            if (value > max) {
                Messages.Add(CompilerMessage.Analyzers.IntegerLiteralOverflow(
                    str, type, expr.Literal.Line
                ));
            }
        }

        void CheckUnsignedIntegerSize (ulong value, ulong max, string type) {
            if (value > max) {
                Messages.Add(CompilerMessage.Analyzers.IntegerLiteralOverflow(
                    str, type, expr.Literal.Line
                ));
            }
        }
    }

    private BoundLiteralExpression ResolveStringLiteralExpression (LiteralExpression expr) {
        if (expr.Literal.TokenKind != TokenKind.String) throw new Exception(
            $"Token kind '{expr.Literal.TokenKind}' is not valid for a string literal"
        );

        // TODO: Right now we can only compile strings without flags that start
        // and end with a single delimiter (" or `).
        var delimiter = expr.Literal.Source[0];
        var str = expr.Literal.Source[1..^1]
            .Replace("\\\\", "\\")
            .Replace("\\n", "\n")
            .Replace("\\r", "\r");

        BoundLiteralExpression bound = new(
            expr, _cmp.Native.Types.String, new ConstantValue(str)
        );

        BoundNodes[expr] = bound;
        return bound;
    }
    #endregion

    /// <summary>
    /// Returns the overload created by the parameter list given. Passing
    /// unbound parameters to this method will not produce an error.
    /// </summary>
    /// <param name="paramList">The list of parameters forming the overload.</param>
    /// <returns></returns>
    public List<TypeSymbol> GetParamTypes (ParameterList paramList) {
        List<TypeSymbol> signature = new();

        foreach (var param in paramList.Parameters) {
            if (TryGetBoundNode(param, out BoundParameter? boundParam) == false) {
                signature.Add(_cmp.Native.Types.Unresolved);
            }
            else {
                signature.Add(boundParam.Symbol.Type ?? _cmp.Native.Types.Unresolved);
            }
        }

        return signature;
    }

    private TypeSymbol GetSuffixTypeInfo (string? suffix) {
        return suffix switch {
            "f64" => _cmp.Native.Types.F64,
            "f32" => _cmp.Native.Types.F32,
            "i64" => _cmp.Native.Types.I64,
            "i32" => _cmp.Native.Types.I32,
            "i16" => _cmp.Native.Types.I16,
            "i8" => _cmp.Native.Types.I8,
            "u64" => _cmp.Native.Types.Ui64,
            "u32" => _cmp.Native.Types.Ui32,
            "u16" => _cmp.Native.Types.Ui16,
            "u8" => _cmp.Native.Types.Ui8,
            "f" => _cmp.Native.Types.Float,
            "i" => _cmp.Native.Types.Int,
            null => _cmp.Native.Types.Num,
            _ => _cmp.Native.Types.Num,
        };
    }
}
