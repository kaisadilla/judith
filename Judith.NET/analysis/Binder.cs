using Judith.NET.analysis.binder;
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
    private Compilation _cmp;

    public Dictionary<SyntaxNode, BoundNode> BoundNodes { get; private set; } = new();

    public Binder (Compilation compilation) {
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

    public BoundFunctionDefinition BindFunctionDefinition (
        FunctionDefinition funcDef, Symbol symbol, SymbolTable scope
    ) {
        if (TryGetBoundNode(funcDef, out BoundFunctionDefinition? boundFuncDef)) {
            return boundFuncDef;
        }

        boundFuncDef = new(funcDef, symbol, scope);
        BoundNodes[funcDef] = boundFuncDef;

        ResolveFunction(boundFuncDef);

        return boundFuncDef;
    }

    public BoundLocalDeclarationStatement BindLocalDeclarationStatement (
        LocalDeclarationStatement localDeclStmt
    ) {
        if (TryGetBoundNode(
            localDeclStmt,
            out BoundLocalDeclarationStatement? boundLocalDeclStmt
        ) == false) {
            boundLocalDeclStmt = new(localDeclStmt);
            BoundNodes[localDeclStmt] = boundLocalDeclStmt;
        }

        TypeInfo implicitType = TypeInfo.UnresolvedType;
        if (localDeclStmt.Initializer != null) {
            if (TryGetBoundNode(localDeclStmt.Initializer.Value, out BoundExpression? expr)) {
                if (TypeInfo.IsResolved(expr.Type)) implicitType = expr.Type;
            }
        }

        foreach (var localDecl in localDeclStmt.DeclaratorList.Declarators) {
            if (TryGetBoundNode(localDecl, out BoundLocalDeclarator? boundLocalDecl) == false) {
                throw new Exception("Local declarator should be bound.");
            }

            if (TypeInfo.IsResolved(boundLocalDecl.Type) == false) {
                ResolveLocalDeclarator(boundLocalDecl, implicitType);
            }

            implicitType = boundLocalDecl.Type ?? TypeInfo.UnresolvedType;
        }

        return boundLocalDeclStmt;
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

    public BoundAssignmentExpression BindAssignmentExpression (
        AssignmentExpression assignmentExpr
    ) {
        if (TryGetBoundNode(assignmentExpr, out BoundAssignmentExpression? boundAssignmentExpr) == false) {
            boundAssignmentExpr = new(assignmentExpr);
            BoundNodes[assignmentExpr] = boundAssignmentExpr;
        }

        if (boundAssignmentExpr.IsComplete == false) {
            ResolveAssignmentExpression(boundAssignmentExpr);
        }

        return boundAssignmentExpr;
    }

    public BoundBinaryExpression BindBinaryExpression (BinaryExpression binaryExpr) {
        if (TryGetBoundNode(binaryExpr, out BoundBinaryExpression? boundBinaryExpr) == false) {
            boundBinaryExpr = new(binaryExpr);
            BoundNodes[binaryExpr] = boundBinaryExpr;
        }

        if (boundBinaryExpr.IsComplete == false) {
            ResolveBinaryExpression(boundBinaryExpr);
        }

        return boundBinaryExpr;
    }

    public BoundLeftUnaryExpression BindLeftUnaryExpression (LeftUnaryExpression leftUnaryExpr) {
        if (TryGetBoundNode(leftUnaryExpr, out BoundLeftUnaryExpression? boundLeftUnaryExpr) == false) {
            boundLeftUnaryExpr = new(leftUnaryExpr);
            BoundNodes[leftUnaryExpr] = boundLeftUnaryExpr;
        }

        if (boundLeftUnaryExpr.IsComplete == false) {
            ResolveLeftUnaryExpression(boundLeftUnaryExpr);
        }

        return boundLeftUnaryExpr;
    }

    public BoundGroupExpression BindGroupExpression (GroupExpression groupExpr) {
        if (TryGetBoundNode(groupExpr, out BoundGroupExpression? boundGroupExpr) == false) {
            boundGroupExpr = new(groupExpr);
            BoundNodes[groupExpr] = boundGroupExpr;
        }

        if (boundGroupExpr.IsComplete == false) {
            ResolveGroupExpression(boundGroupExpr);
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

        if (boundIdExpr.IsComplete == false) {
            ResolveIdentifierExpression(boundIdExpr);
        }

        return boundIdExpr;
    }

    public BoundIdentifierExpression BindIdentifierExpression (IdentifierExpression idExpr) {
        if (TryGetBoundNode(idExpr, out BoundIdentifierExpression? boundIdExpr) == false) {
            throw new Exception("IdentifierExpression should be bound.");
        }

        if (boundIdExpr.IsComplete == false) {
            ResolveIdentifierExpression(boundIdExpr);
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
        TypeAnnotation typeAnnt, Symbol symbol
    ) {
        if (TryGetBoundNode(typeAnnt, out BoundTypeAnnotation? boundTypeAnnt) == false) {
            boundTypeAnnt = new(typeAnnt, symbol);
            BoundNodes[typeAnnt] = boundTypeAnnt;
        }

        return boundTypeAnnt;
    }

    #region Resolve literals
    private BoundLiteralExpression ResolveBooleanLiteralExpression (LiteralExpression expr) {
        BoundLiteralExpression bound;
        if (expr.Literal.TokenKind == TokenKind.KwTrue) {
            bound = new(expr, GetTypeInfo("Bool"), new ConstantValue(true));
        }
        else if (expr.Literal.TokenKind ==TokenKind.KwFalse) {
            bound = new(expr, GetTypeInfo("Bool"), new ConstantValue(false));
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

                bound = new(expr, _cmp.Native.Types.Num, new((double)value)); // F64
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

                bound = new(expr, _cmp.Native.Types.Num, new((double)value)); // F32
            }
            else {
                Messages.Add(CompilerMessage.Analyzers.NumberSuffixCannotBeUsedForDecimal(
                    suffix, expr.Literal.Line
                ));
                bound = new(expr, _cmp.Native.Types.Num, new(0d)); // F64
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
                    bound = new(expr, _cmp.Native.Types.Num, new((double)value)); // U64 / (ulong)
                }
                else if (suffix == "i32") {
                    CheckUnsignedIntegerSize(value, int.MaxValue, "U32");
                    bound = new(expr, _cmp.Native.Types.Num, new((double)value)); // U32 / (ulong)
                }
                else if (suffix == "i16") {
                    CheckUnsignedIntegerSize(value, int.MaxValue, "U16");
                    bound = new(expr, _cmp.Native.Types.Num, new((double)value)); // U16 / (ulong)
                }
                else if (suffix == "i8") {
                    CheckUnsignedIntegerSize(value, int.MaxValue, "U8");
                    bound = new(expr, _cmp.Native.Types.Num, new((double)value)); // U8 / (ulong)
                }
                else {
                    bound = new(expr, _cmp.Native.Types.Num, new((double)value)); //GetSuffixTypeInfo(suffix) / (ulong)
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
                    bound = new(expr, _cmp.Native.Types.Num, new((double)value)); // I64 / (long)
                }
                else if (suffix == "i32") {
                    CheckIntegerSize(value, int.MaxValue, "I32");
                    bound = new(expr, _cmp.Native.Types.Num, new((double)value)); // I32 / (long)
                }
                else if (suffix == "i16") {
                    CheckIntegerSize(value, short.MaxValue, "I16");
                    bound = new(expr, _cmp.Native.Types.Num, new((double)value)); // I16 / (long)
                }
                else if (suffix == "i8") {
                    CheckIntegerSize(value, short.MaxValue, "I8");
                    bound = new(expr, _cmp.Native.Types.Num, new((double)value)); // I8 / (long)
                }
                else {
                    bound = new(expr, _cmp.Native.Types.Num, new((double)value)); // GetSuffixTypeInfo(suffix) / (long)
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
            expr, GetTypeInfo("String"), new ConstantValue(str)
        );

        BoundNodes[expr] = bound;
        return bound;
    }
    #endregion

    #region Resolve methods
    public void ResolveFunction (BoundFunctionDefinition boundFuncDef) {
        ResolveFunctionReturnType(boundFuncDef);
    }

    public void ResolveFunctionReturnType (BoundFunctionDefinition boundFuncDef) {
        if (
            boundFuncDef.ReturnType != null
            && boundFuncDef.ReturnType != TypeInfo.UnresolvedType
        ) {
            return;
        }

        if (boundFuncDef.Node.ReturnTypeAnnotation == null) {
            // TODO: Infer from body.
        }
        else {
            if (TryGetBoundNode(
                boundFuncDef.Node.ReturnTypeAnnotation,
                out BoundNode? boundReturnType
            ) == false) {
                return;
            }

            // TODO
        }
    }

    private void ResolveBinaryExpression (BoundBinaryExpression boundBinaryExpr) {
        switch (boundBinaryExpr.Node.Operator.OperatorKind) {
            // Math - their type is determined by the operator function they call,
            // except for natively defined operations (e.g. I64 + I64).
            case OperatorKind.Add:
            case OperatorKind.Subtract:
            case OperatorKind.Multiply:
            case OperatorKind.Divide:
                ResolveMathOperation();
                return;

            // Comparisons - they always return bool.
            case OperatorKind.Equals:
            case OperatorKind.NotEquals:
            case OperatorKind.Like:
            case OperatorKind.ReferenceEquals:
            case OperatorKind.ReferenceNotEquals:
            case OperatorKind.LessThan:
            case OperatorKind.LessThanOrEqualTo:
            case OperatorKind.GreaterThan:
            case OperatorKind.GreaterThanOrEqualTo:
            case OperatorKind.LogicalAnd:
            case OperatorKind.LogicalOr:
                boundBinaryExpr.Type = GetTypeInfo("Bool");
                return;

            // Operators that aren't used in binary expressions:
            case OperatorKind.Assignment:
            case OperatorKind.MemberAccess:
            case OperatorKind.ScopeResolution:
            case OperatorKind.BitwiseNot:
                throw new Exception(
                    $"Invalid operator for a binary expression: " +
                    $"'{boundBinaryExpr.Node.Operator.OperatorKind}'."
                );
        }

        void ResolveMathOperation () {
            var opKind = boundBinaryExpr.Node.Operator.OperatorKind;

            TryGetBoundNode(boundBinaryExpr.Node.Left, out BoundExpression? boundLeft);
            TryGetBoundNode(boundBinaryExpr.Node.Right, out BoundExpression? boundRight);

            if (boundLeft == null 
                || boundRight == null
                || TypeInfo.IsResolved(boundLeft.Type) == false
                || TypeInfo.IsResolved(boundRight.Type) == false
            ) {
                boundBinaryExpr.Type = TypeInfo.UnresolvedType;
                return;
            }

            if (
                _cmp.Native.IsNumericType(boundLeft.Type)
                && _cmp.Native.IsNumericType(boundRight.Type)
            ) {
                boundBinaryExpr.Type = _cmp.Native.CoalesceNumericTypes(
                    boundLeft.Type, boundRight.Type
                );
                return;
            }

            // TODO: Implement and check defined operations and stuff.
            if (boundLeft.Type == boundRight.Type) {
                boundBinaryExpr.Type = boundLeft.Type;
                return;
            }

            boundBinaryExpr.Type = TypeInfo.ErrorType;
        }
    }

    private void ResolveAssignmentExpression (BoundAssignmentExpression boundAssignmentExpr) {
        if (TryGetBoundNode(boundAssignmentExpr.Node.Right, out BoundExpression? boundExpr)) {
            if (TypeInfo.IsResolved(boundExpr.Type)) {
                boundAssignmentExpr.Type = boundExpr.Type;
                return;
            }
        }

        boundAssignmentExpr.Type = TypeInfo.UnresolvedType;
    }

    private void ResolveLeftUnaryExpression (BoundLeftUnaryExpression boundLeftUnaryExpr) {
        if (TryGetBoundNode(boundLeftUnaryExpr.Node.Expression, out BoundExpression? boundExpr)) {
            if (TypeInfo.IsResolved(boundExpr.Type)) {
                boundLeftUnaryExpr.Type = boundExpr.Type;
                return;
            }
        }

        boundLeftUnaryExpr.Type = TypeInfo.UnresolvedType;
    }

    private void ResolveGroupExpression (BoundGroupExpression boundGroupExpr) {
        if (TryGetBoundNode(boundGroupExpr.Node.Expression, out BoundExpression? boundExpr)) {
            if (TypeInfo.IsResolved(boundExpr.Type)) {
                boundGroupExpr.Type = boundExpr.Type;
                return;
            }
        }

        boundGroupExpr.Type = TypeInfo.UnresolvedType;
    }

    private void ResolveIdentifierExpression (BoundIdentifierExpression boundIdExpr) {
        if (TypeInfo.IsResolved(boundIdExpr.Symbol.Type)) {
            boundIdExpr.Type = boundIdExpr.Symbol.Type;
        }
        else {
            boundIdExpr.Type = TypeInfo.UnresolvedType;
        }
    }

    private void ResolveLocalDeclarator (
        BoundLocalDeclarator boundLocalDecl, TypeInfo inferredType
    ) {
        if (boundLocalDecl.Node.TypeAnnotation == null) {
            boundLocalDecl.Type = inferredType;
            boundLocalDecl.Symbol.Type = boundLocalDecl.Type;
            return;
        }

        if (TryGetBoundNode(
            boundLocalDecl.Node.TypeAnnotation,
            out BoundTypeAnnotation? typeAnnt
        ) == false) {
            throw new Exception("Type annotation should be bound.");
        }

        if (_cmp.TypeTable.TryGetType(
            typeAnnt.Symbol.FullyQualifiedName,
            out TypeInfo? type
        ) == false) {
            Messages.Add(CompilerMessage.Analyzers.TypeDoesntExist(
                typeAnnt.Symbol.FullyQualifiedName,
                typeAnnt.Node.Line
            ));
        }

        boundLocalDecl.Type = type;
        boundLocalDecl.Symbol.Type = boundLocalDecl.Type;
    }
    #endregion

    private TypeInfo GetTypeInfo (string type) {
        if (_cmp.TypeTable.TryGetType(type, out var typeInfo)) {
            return typeInfo;
        }

        throw new Exception($"Couldn't find type '{type}' for native literal!");
    }

    private TypeInfo GetSuffixTypeInfo (string? suffix) {
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
