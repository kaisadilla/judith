using Judith.NET.analysis;
using Judith.NET.analysis.binder;
using Judith.NET.analysis.semantics;
using Judith.NET.analysis.syntax;
using Judith.NET.ir.syntax;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Judith.NET.ir;

public class JudithIRGenerator {
    private JudithCompilation _cmp;
    private IRNativeHeader _native;

    private IRNativeHeader.TypeCollection NativeTypes => _native.TypeRefs;

    public IRProgram? Program { get; private set; } = null;

    public JudithIRGenerator (JudithCompilation cmp, IRNativeHeader nativeHeader) {
        _cmp = cmp;
        _native = nativeHeader;
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

        string returnType = boundNode.Symbol.ReturnType.FullyQualifiedName;
        var body = CompileFunctionBody(node.Body);
        var kind = IRFunctionKind.Function; // TODO.
        bool isMethod = false; // TODO.

        return new(name, parameters, returnType, body, kind, isMethod);
    }

    private IRParameter CompileParameter (Parameter node) {
        var boundNode = Bound<BoundParameter>(node);
        if (boundNode.Symbol.Type == null) ThrowIncompleteNode(node);

        string name = boundNode.Symbol.Name;
        string type = boundNode.Symbol.Type.Name;
        var mutability = node.Declarator.LocalKind switch {
            LocalKind.Variable => IRMutability.Variable,
            _ => IRMutability.Constant,
        };

        return new(name, type, mutability);
    }

    private List<IRStatement> CompileFunctionBody (BlockStatement blockStmt) {
        List<IRStatement> statements = [];

        foreach (var node in blockStmt.Nodes) {
            if (node is not Statement stmt) throw new NotImplementedException(
                "Non-statement nodes in functions are not implemented yet."
            );

            statements.AddRange(CompileStatement(stmt));
        }

        return statements;
    }

    private List<IRStatement> CompileBlockStatement (BlockStatement blockStmt) {
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
        if (localDeclStmt.DeclaratorList.Declarators.Count != 1) {
            throw new NotImplementedException("Multiple declaration not yet supported.");
        }
        if (localDeclStmt.DeclaratorList.DeclaratorKind != LocalDeclaratorKind.Regular) {
            throw new NotImplementedException("Declarator kind not yet supported.");
        }

        var boundDecl = Bound<BoundLocalDeclarator>(
            localDeclStmt.DeclaratorList.Declarators[0]
        );
        if (boundDecl.Symbol.Type == null) ThrowIncompleteNode(localDeclStmt);

        string name = boundDecl.Symbol.Name;
        string type = boundDecl.Symbol.Type.FullyQualifiedName;
        var mutability = localDeclStmt.DeclaratorList.Declarators[0].LocalKind switch {
            LocalKind.Variable => IRMutability.Variable,
            _ => IRMutability.Constant,
        };

        IRExpression? init = null;
        if (localDeclStmt.Initializer != null) {
            init = CompileExpression(localDeclStmt.Initializer.Value);
        }

        return [new IRLocalDeclarationStatement(name, type, mutability, init)];
    }

    private List<IRStatement> CompileReturnStatement (ReturnStatement returnStmt) {
        IRExpression? returnExpr = null;
        if (returnStmt.Expression != null) {
            returnExpr = CompileExpression(returnStmt.Expression);
        }

        return [new IRReturnStatement(returnExpr)];
    }

    private List<IRStatement> CompileExpressionStatement (ExpressionStatement exprStmt) {
        var expr = exprStmt.Expression;

        var irExpr = CompileExpression(expr);
        var irStmt = new IRExpressionStatement(irExpr);

        return [irStmt];
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
        //if (boundExpr.Type == null) ThrowIncompleteNode(node);

        string type = "TODO";// TODO: boundExpr.Type.FullyQualifiedName;
        IRExpression test = CompileExpression(node.Test);

        List<IRStatement> consequent;
        List<IRStatement>? alternate;

        if (node.Consequent is BlockStatement consequentBlockStmt) {
            consequent = CompileBlockStatement(consequentBlockStmt);
        }
        else {
            throw new NotImplementedException("Cannot compile non-block statements yet.");
        }

        if (node.Alternate == null) {
            alternate = null;
        }
        else if (node.Alternate is BlockStatement alternateBlockStmt) {
            alternate = CompileBlockStatement(alternateBlockStmt);
        }
        else {
            throw new NotImplementedException("Cannot compile non-block statements yet.");
        }

        return new(test, consequent, alternate, type);
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
        //if (boundExpr.Type == null) ThrowIncompleteNode(node);

        string type = "TODO";// TODO: boundExpr.Type.FullyQualifiedName;
        IRExpression test = CompileExpression(node.Test);

        if (node.Body is BlockStatement blockStmt) {
            var body = CompileBlockStatement(blockStmt);

            return new IRWhileExpression(test, body, type);
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

        return new(left, right, type);
    }

    private IRExpression CompileBinaryExpression (BinaryExpression expr) {
        var boundExpr = Bound<BoundBinaryExpression>(expr);
        if (boundExpr.Type == null) ThrowIncompleteNode(expr);

        string type = boundExpr.Type.FullyQualifiedName;
        var left = CompileExpression(expr.Left);
        var right = CompileExpression(expr.Right);

        switch (expr.Operator.OperatorKind) {
            case OperatorKind.Add:
                return CompileBinaryAddExpression(left, right, type, IRMathOperation.Add);
            case OperatorKind.Subtract:
                return CompileBinaryAddExpression(left, right, type, IRMathOperation.Subtract);
            case OperatorKind.Multiply:
                return CompileBinaryAddExpression(left, right, type, IRMathOperation.Multiply);
            case OperatorKind.Divide:
                return CompileBinaryAddExpression(left, right, type, IRMathOperation.Divide);
            case OperatorKind.Equals:
                return CompileComparisonExpression(left, right, type, IRComparisonOperation.Equals);
            case OperatorKind.NotEquals:
                return CompileComparisonExpression(left, right, type, IRComparisonOperation.NotEquals);
            case OperatorKind.ReferenceEquals:
                throw new NotImplementedException("Reference comparison not yet implemented.");
            case OperatorKind.ReferenceNotEquals:
                throw new NotImplementedException("Reference comparison not yet implemented.");
            case OperatorKind.LessThan:
                return CompileComparisonExpression(left, right, type, IRComparisonOperation.LessThan);
            case OperatorKind.LessThanOrEqualTo:
                return CompileComparisonExpression(left, right, type, IRComparisonOperation.LessThanOrEqualTo);
            case OperatorKind.GreaterThan:
                return CompileComparisonExpression(left, right, type, IRComparisonOperation.GreaterThan);
            case OperatorKind.GreaterThanOrEqualTo:
                return CompileComparisonExpression(left, right, type, IRComparisonOperation.GreaterThanOrEqualTo);
            default:
                throw new NotImplementedException(
                    $"Cannot yet compile binary expression with operator " +
                    $"'{expr.Operator.OperatorKind}'."
                );
        }
    }
    
    private IRExpression CompileUnaryExpression (UnaryExpression node) {
        var boundNode = Bound<BoundUnaryExpression>(node);
        if (boundNode.Type == null) ThrowIncompleteNode(node);

        string type = boundNode.Type.FullyQualifiedName;
        var expr = CompileExpression(node.Expression);

        switch (node.Operator.OperatorKind) {
            case OperatorKind.Subtract:
                return CompileUnaryAddExpression(expr, type, IRUnaryOperation.Negate);
            default:
                throw new NotImplementedException(
                    $"Cannot yet compile binary expression with operator " +
                    $"'{node.Operator.OperatorKind}'."
                );
        }
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

    private IRCallExpression CompileCallExpression (CallExpression expr) {
        var boundExpr = Bound<BoundCallExpression>(expr);
        if (boundExpr.Type == null) ThrowIncompleteNode(expr);

        string type = boundExpr.Type.FullyQualifiedName;
        var callee = CompileExpression(expr.Callee);
        List<IRArgument> arguments = [];

        foreach (var arg in expr.Arguments.Arguments) {
            arguments.Add(CompileArgument(arg));
        }

        return new(callee, arguments, type);
    }

    private IRArgument CompileArgument (Argument arg) {
        var expr = CompileExpression(arg.Expression);

        return new(expr);
    }

    private IRIdentifierExpression CompileIdentifierExpression (IdentifierExpression expr) {
        var boundExpr = Bound<BoundIdentifierExpression>(expr);
        if (boundExpr.Type == null) ThrowIncompleteNode(expr);

        string type = boundExpr.Type.FullyQualifiedName;

        if (
            boundExpr.Symbol.Kind == SymbolKind.Local
            || boundExpr.Symbol.Kind == SymbolKind.Parameter
        ) {
            return new(
                boundExpr.Symbol.Name,
                IRIdentifierKind.Local,
                type
            );
        }
        else {
            return new(
                boundExpr.Symbol.FullyQualifiedName,
                IRIdentifierKind.Global,
                type
            );
        }
    }

    private IRLiteralExpression CompileLiteralExpression (LiteralExpression expr) {
        var boundExpr = Bound<BoundLiteralExpression>(expr);
        if (boundExpr.Type == null) ThrowIncompleteNode(expr);

        var value = boundExpr.Value;
        var type = boundExpr.Type.FullyQualifiedName;

        return new IRLiteralExpression(value, type);
    }

    private List<IRStatement> Compile_P_PrintStatement (P_PrintStatement pStmt) {
        var expr = CompileExpression(pStmt.Expression);

        return [new IR_P_PrintStatement(expr)];
    }

    [DoesNotReturn]
    private static void ThrowIncompleteNode (SyntaxNode node) {
        throw new($"{node} is not complete.");
    }

    private T Bound<T> (SyntaxNode node) where T : BoundNode {
        return _cmp.Binder.GetBoundNodeOrThrow<T>(node);
    }
}
