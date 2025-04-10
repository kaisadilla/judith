using Judith.NET.analysis;
using Judith.NET.analysis.binder;
using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using Judith.NET.debugging;
using Judith.NET.ir.syntax;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Judith.NET.ir;

public class JudithIRGenerator {
    private JudithCompilation _cmp;
    private IRNativeHeader _native;
    private DebuggingInfo? _debugInfo = null;

    private IRNativeHeader.TypeCollection NativeTypes => _native.TypeRefs;

    public IRProgram? Program { get; private set; } = null;

    public JudithIRGenerator (JudithCompilation cmp, IRNativeHeader nativeHeader) {
        _cmp = cmp;
        _native = nativeHeader;
    }

    public void SetDebuggingInfo (DebuggingInfo? info) {
        _debugInfo = info;
    }

    [MemberNotNull(nameof(Program))]
    public void Generate () {
        Program = new() {
            NativeHeader = _native,
            Dependencies = [], // TODO
        };

        foreach (var cu in _cmp.Program.Units) {
            Program.Blocks.Add(GenerateBlock(cu));
        }
    }

    private IRBlock GenerateBlock (CompilerUnit unit) {
        IRBlock block = new(unit.FileName);

        if (unit.ImplicitFunction != null) {
            block.AddFunction(CompileFunction(unit.ImplicitFunction));
        }

        foreach (var item in unit.TopLevelItems) {
            if (item.Kind == SyntaxKind.FunctionDefinition) {
                block.AddFunction(CompileFunction((FunctionDefinition)item));
            }
        }

        return block;
    }

    private IRFunction CompileFunction (FunctionDefinition node) {
        var boundNode = Bound<BoundFunctionDefinition>(node);
        if (boundNode.Symbol.ReturnType == null) ThrowIncompleteNode(node);

        string name = boundNode.Symbol.FullyQualifiedName;

        List<IRParameter> parameters = [];

        foreach (var param in node.Parameters.Parameters) {
            parameters.Add(CompileParameter(param));
        }

        IRTypeName returnType = new(
            IRTypeKind.Regular,
            boundNode.Symbol.ReturnType.FullyQualifiedName
        );
        var body = CompileFunctionBody(node.Body);
        var kind = IRFunctionKind.Function; // TODO.
        bool isMethod = false; // TODO.

        IRFunction irFunc = new(name, parameters, returnType, body, kind, isMethod);
        LinkToSource(irFunc, node);

        return irFunc;
    }

    private IRParameter CompileParameter (Parameter node) {
        var boundNode = Bound<BoundParameter>(node);
        if (boundNode.Symbol.Type == null) ThrowIncompleteNode(node);

        string name = boundNode.Symbol.Name;
        IRTypeName type = new(
            IRTypeKind.Regular,
            boundNode.Symbol.Type.FullyQualifiedName
        );
        bool isFinal = node.Declarator.LocalKind == LocalKind.Constant;

        IRParameter irParam = new(name, type, isFinal);
        LinkToSource(irParam, node);

        return irParam;
    }

    private List<IRStatement> CompileFunctionBody (BlockBody blockStmt) {
        List<IRStatement> statements = [];

        foreach (var node in blockStmt.Nodes) {
            if (node is not Statement stmt) throw new NotImplementedException(
                "Non-statement nodes in functions are not implemented yet."
            );

            statements.AddRange(CompileStatement(stmt));
        }

        return statements;
    }

    private List<IRStatement> CompileBlockStatement (BlockBody blockStmt) {
        List<IRStatement> statements = [];

        foreach (var node in blockStmt.Nodes) {
            if (node is not Statement stmt) throw new(
                "Regular block statements can only contain statements."
            );

            statements.AddRange(CompileStatement(stmt));
        }

        return statements;
    }

    private List<IRStatement> CompileStatement (Statement stmt) {
        switch (stmt.Kind) {
            case SyntaxKind.LocalDeclarationStatement:
                return CompileLocalDeclarationStatement((LocalDeclarationStatement)stmt);
            case SyntaxKind.ReturnStatement:
                return CompileReturnStatement((ReturnStatement)stmt);
            case SyntaxKind.YieldStatement:
                return CompileYieldStatement((YieldStatement)stmt);
            case SyntaxKind.ExpressionStatement:
                return CompileExpressionStatement((ExpressionStatement)stmt);
            case SyntaxKind.P_PrintStatement:
                return Compile_P_PrintStatement((P_PrintStatement)stmt);
            default:
                throw new NotImplementedException(
                    $"Statement of kind '{stmt.Kind}' is not implemented yet."
                );
        }
    }

    private List<IRStatement> CompileLocalDeclarationStatement (
        LocalDeclarationStatement localDeclStmt
    ) {
        // TODO: Multiple declarations.
        if (localDeclStmt.DeclaratorList.DeclaratorKind != LocalDeclaratorKind.Regular) {
            throw new NotImplementedException("Declarator kind not yet supported.");
        }
        if (localDeclStmt.DeclaratorList.Declarators.Count != 1) {
            throw new NotImplementedException("Multiple declaration not yet supported.");
        }

        var boundDecl = Bound<BoundLocalDeclarator>(
            localDeclStmt.DeclaratorList.Declarators[0]
        );
        if (boundDecl.Symbol.Type == null) ThrowIncompleteNode(localDeclStmt);

        string name = boundDecl.Symbol.Name;
        string type = boundDecl.Symbol.Type.FullyQualifiedName;
        bool isFinal = localDeclStmt.DeclaratorList.Declarators[0].LocalKind == LocalKind.Constant;
        bool isImmutable = false;

        IRExpression? init = null;
        if (localDeclStmt.Initializer != null) {
            init = CompileExpression(localDeclStmt.Initializer.Values[0]);
        }

        IRLocalDeclarationStatement irLocalDecl = new(name, type, isFinal, isImmutable, init);
        LinkToSource(irLocalDecl, localDeclStmt);

        return [irLocalDecl];
    }

    private List<IRStatement> CompileReturnStatement (ReturnStatement returnStmt) {
        IRExpression? returnedExpr = null;
        if (returnStmt.Expression != null) {
            returnedExpr = CompileExpression(returnStmt.Expression);
        }

        IRReturnStatement irReturnStmt = new(returnedExpr);
        LinkToSource(irReturnStmt, returnStmt);

        return [irReturnStmt];
    }

    private List<IRStatement> CompileYieldStatement (YieldStatement yieldStmt) {
        IRExpression yieldedExpr = CompileExpression(yieldStmt.Expression);

        var irYieldStmt = new IRYieldStatement(yieldedExpr);
        LinkToSource(irYieldStmt, yieldStmt);

        return [irYieldStmt];
    }

    private List<IRStatement> CompileExpressionStatement (ExpressionStatement exprStmt) {
        var irExpr = CompileExpression(exprStmt.Expression);

        IRExpressionStatement irExprStmt = new(irExpr);
        LinkToSource(irExprStmt, exprStmt);

        return [irExprStmt];
    }

    private IRExpression CompileExpression (Expression expr) {
        switch (expr.Kind) {
            case SyntaxKind.IfExpression:
                return CompileIfExpression((IfExpression)expr);
            case SyntaxKind.LoopExpression:
                return CompileLoopExpression((LoopExpression)expr);
            case SyntaxKind.WhileExpression:
                return CompileWhileExpression((WhileExpression)expr);
            case SyntaxKind.AssignmentExpression: 
                return CompileAssignmentExpression((AssignmentExpression)expr);
            case SyntaxKind.BinaryExpression:
                return CompileBinaryExpression((BinaryExpression)expr);
            case SyntaxKind.LeftUnaryExpression:
                return CompileUnaryExpression((UnaryExpression)expr);
            case SyntaxKind.GroupExpression:
                return CompileExpression(((GroupExpression)expr).Expression);
            case SyntaxKind.CallExpression: 
                return CompileCallExpression((CallExpression)expr);
            case SyntaxKind.IdentifierExpression:
                return CompileIdentifierExpression((IdentifierExpression)expr);
            case SyntaxKind.LiteralExpression:
                return CompileLiteralExpression((LiteralExpression)expr);
            default: {
                throw new NotImplementedException(
                    $"Cannot yet compile expression containing '{expr.Kind}'."
                );
            }
        }
    }

    private IRIfExpression CompileIfExpression (IfExpression node) {
        var boundExpr = Bound<BoundIfExpression>(node);
        if (boundExpr.Type == null) ThrowIncompleteNode(node);

        string type = boundExpr.Type.FullyQualifiedName;
        IRExpression test = CompileExpression(node.Test);

        List<IRStatement> consequent;
        List<IRStatement>? alternate;

        if (node.Consequent is BlockBody consequentBlockStmt) {
            consequent = CompileBlockStatement(consequentBlockStmt);
        }
        else {
            throw new NotImplementedException("Cannot compile non-block statements yet.");
        }

        if (node.Alternate == null) {
            alternate = null;
        }
        else if (node.Alternate is BlockBody alternateBlockStmt) {
            alternate = CompileBlockStatement(alternateBlockStmt);
        }
        else {
            throw new NotImplementedException("Cannot compile non-block statements yet.");
        }

        IRIfExpression irIfExpr = new(test, consequent, alternate, type);
        LinkToSource(irIfExpr, node);

        return irIfExpr;
    }

    private IRWhileExpression CompileLoopExpression (LoopExpression node) {
        throw new NotImplementedException("Loop not yet implemented.");
        //var boundExpr = Bound<BoundLoopExpression>(node);
        //if (boundExpr.Type == null) ThrowIncompleteNode(node);
        //
        //string type = boundExpr.Type.FullyQualifiedName;
        //IRLiteralExpression test = new(new(true), _native.TypeRefs.Bool.Name);
        //
        //if (node.Body is BlockStatement blockStmt) {
        //    var body = CompileBlockStatement(blockStmt);
        //
        //    return new IRWhileExpression(test, body, type);
        //}
        //else {
        //    throw new NotImplementedException("Cannot compile non-block statements yet.");
        //}
    }

    private IRWhileExpression CompileWhileExpression (WhileExpression node) {
        var boundExpr = Bound<BoundWhileExpression>(node);
        if (boundExpr.Type == null) ThrowIncompleteNode(node);

        string type = boundExpr.Type.FullyQualifiedName;
        IRExpression test = CompileExpression(node.Test);

        if (node.Body is BlockBody blockStmt) {
            var body = CompileBlockStatement(blockStmt);

            IRWhileExpression irWhileExpr = new(test, body, type);
            LinkToSource(irWhileExpr, node);

            return irWhileExpr;
        }
        else {
            throw new NotImplementedException("Cannot compile non-block statements yet.");
        }
    }

    private IRAssignmentExpression CompileAssignmentExpression (AssignmentExpression expr) {
        var boundExpr = Bound<BoundAssignmentExpression>(expr);
        if (boundExpr.Type == null) ThrowIncompleteNode(expr);

        string type = boundExpr.Type.FullyQualifiedName;
        var left = CompileExpression(expr.Left);
        var right = CompileExpression(expr.Right);

        IRAssignmentExpression irAssignmentExpr = new(left, right, type);
        LinkToSource(irAssignmentExpr, expr);

        return irAssignmentExpr;
    }

    private IRExpression CompileBinaryExpression (BinaryExpression node) {
        var boundExpr = Bound<BoundBinaryExpression>(node);
        if (boundExpr.Type == null) ThrowIncompleteNode(node);

        string type = boundExpr.Type.FullyQualifiedName;
        var left = CompileExpression(node.Left);
        var right = CompileExpression(node.Right);

        IRExpression irExpr;

        switch (node.Operator.OperatorKind) {
            case OperatorKind.Add:
                irExpr = CompileBinaryAddExpression (left, right, type, IRMathOperation.Add);
                break;
            case OperatorKind.Subtract:
                irExpr = CompileBinaryAddExpression(left, right, type, IRMathOperation.Subtract);
                break;
            case OperatorKind.Multiply:
                irExpr = CompileBinaryAddExpression(left, right, type, IRMathOperation.Multiply);
                break;
            case OperatorKind.Divide:
                irExpr = CompileBinaryAddExpression(left, right, type, IRMathOperation.Divide);
                break;
            case OperatorKind.Equals:
                irExpr = CompileComparisonExpression(left, right, type, IRComparisonOperation.Equals);
                break;
            case OperatorKind.NotEquals:
                irExpr = CompileComparisonExpression(left, right, type, IRComparisonOperation.NotEquals);
                break;
            case OperatorKind.ReferenceEquals:
                throw new NotImplementedException("Reference comparison not yet implemented.");
            case OperatorKind.ReferenceNotEquals:
                throw new NotImplementedException("Reference comparison not yet implemented.");
            case OperatorKind.LessThan:
                irExpr = CompileComparisonExpression(left, right, type, IRComparisonOperation.LessThan);
                break;
            case OperatorKind.LessThanOrEqualTo:
                irExpr = CompileComparisonExpression(left, right, type, IRComparisonOperation.LessThanOrEqualTo);
                break;
            case OperatorKind.GreaterThan:
                irExpr = CompileComparisonExpression(left, right, type, IRComparisonOperation.GreaterThan);
                break;
            case OperatorKind.GreaterThanOrEqualTo:
                irExpr = CompileComparisonExpression(left, right, type, IRComparisonOperation.GreaterThanOrEqualTo);
                break;
            default:
                throw new NotImplementedException(
                    $"Cannot yet compile binary expression with operator " +
                    $"'{node.Operator.OperatorKind}'."
                );
        }

        LinkToSource(irExpr, node);

        return irExpr;
    }
    
    private IRExpression CompileUnaryExpression (UnaryExpression node) {
        var boundNode = Bound<BoundUnaryExpression>(node);
        if (boundNode.Type == null) ThrowIncompleteNode(node);

        string type = boundNode.Type.FullyQualifiedName;
        var expr = CompileExpression(node.Expression);

        IRExpression irExpr;

        switch (node.Operator.OperatorKind) {
            case OperatorKind.Subtract:
                irExpr = CompileUnaryAddExpression(expr, type, IRUnaryOperation.Negate);
                break;
            default:
                throw new NotImplementedException(
                    $"Cannot yet compile binary expression with operator " +
                    $"'{node.Operator.OperatorKind}'."
                );
        }

        LinkToSource(irExpr, node);

        return irExpr;
    }

    private IRExpression CompileBinaryAddExpression (
        IRExpression left, IRExpression right, string type, IRMathOperation op
    ) {
        if (left.Type == NativeTypes.F64.Name && right.Type == NativeTypes.F64.Name) {
            return new IRMathBinaryExpression(left, right, op, type);
        }

        throw new NotImplementedException(
            "Overloaded binary operators not yet implemented."
        );
    }

    public IRExpression CompileComparisonExpression (
        IRExpression left, IRExpression right, string type, IRComparisonOperation op
    ) {
        if (left.Type == NativeTypes.F64.Name && right.Type == NativeTypes.F64.Name) {
            return new IRComparisonExpression(left, right, op, type);
        }

        throw new NotImplementedException(
            "Overloaded comparison operators not yet implemented."
        );
    }

    private IRExpression CompileUnaryAddExpression(
        IRExpression expr, string type, IRUnaryOperation op
    ) {
        if (expr.Type == NativeTypes.F64.Name) {
            return new IRMathUnaryExpression(expr, op, type);
        }

        throw new NotImplementedException(
            "Overloaded unary operators not yet implemented."
        );
    }

    private IRCallExpression CompileCallExpression (CallExpression node) {
        var boundExpr = Bound<BoundCallExpression>(node);
        if (boundExpr.Type == null) ThrowIncompleteNode(node);

        string type = boundExpr.Type.FullyQualifiedName;
        var callee = CompileExpression(node.Callee);
        List<IRArgument> arguments = [];

        foreach (var arg in node.Arguments.Arguments) {
            arguments.Add(CompileArgument(arg));
        }

        IRCallExpression irCallExpr = new(callee, arguments, type);
        LinkToSource(irCallExpr, node);

        return irCallExpr;
    }

    private IRArgument CompileArgument (Argument node) {
        var expr = CompileExpression(node.Expression);

        IRArgument irArg = new(expr);
        LinkToSource(irArg, node);

        return irArg;
    }

    private IRIdentifierExpression CompileIdentifierExpression (IdentifierExpression node) {
        var boundExpr = Bound<BoundIdentifierExpression>(node);
        if (boundExpr.Type == null) ThrowIncompleteNode(node);

        string type = boundExpr.Type.FullyQualifiedName;

        IRIdentifierExpression irIdExpr;

        if (
            boundExpr.Symbol.Kind == SymbolKind.Local
            || boundExpr.Symbol.Kind == SymbolKind.Parameter
        ) {
            irIdExpr = new(
                boundExpr.Symbol.Name,
                IRIdentifierKind.Local,
                type
            );
        }
        else {
            irIdExpr = new(
                boundExpr.Symbol.FullyQualifiedName,
                IRIdentifierKind.Global,
                type
            );
        }

        LinkToSource(irIdExpr, node);

        return irIdExpr;
    }

    private IRLiteralExpression CompileLiteralExpression (LiteralExpression node) {
        var boundExpr = Bound<BoundLiteralExpression>(node);
        if (boundExpr.Type == null) ThrowIncompleteNode(node);

        var value = boundExpr.Value;
        var type = boundExpr.Type.FullyQualifiedName;

        IRLiteralExpression irLiteralExpr = new(value, type);
        LinkToSource(irLiteralExpr, node);

        return irLiteralExpr;
    }

    private List<IRStatement> Compile_P_PrintStatement (P_PrintStatement node) {
        var expr = CompileExpression(node.Expression);

        IR_P_PrintStatement ir_P_PrintStmt = new(expr);
        LinkToSource(ir_P_PrintStmt, node);

        return [ir_P_PrintStmt];
    }

    [DoesNotReturn]
    private static void ThrowIncompleteNode (SyntaxNode node) {
        throw new($"{node} is not complete.");
    }

    private T Bound<T> (SyntaxNode node) where T : BoundNode {
        return _cmp.Binder.GetBoundNodeOrThrow<T>(node);
    }

    /// <summary>
    /// When the debugger info is provided, links the IR node given to the
    /// Judith node given.
    /// </summary>
    /// <param name="irNode">The IR node to link.</param>
    /// <param name="judithNode">The Judith node to link.</param>
    private void LinkToSource (IRNode irNode, SyntaxNode judithNode) {
        if (_debugInfo == null) return;

        _debugInfo.IRNodeMap[irNode] = judithNode;
    }
}
