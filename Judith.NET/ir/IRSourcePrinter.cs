using Judith.NET.ir.syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir;

public class IRSourcePrinter {
    private readonly IRBlock _block;
    private StringBuilder _buffer = new();
    private int _indentation = 0;

    public int SpacesPerIndent { get; init; } = 4;

    public string? Source { get; private set; }

    public IRSourcePrinter (IRBlock block) {
        _block = block;
    }

    [MemberNotNull(nameof(Source))]
    public void Print () {
        _buffer.Clear();
        foreach (var func in _block.Functions) {
            PrintFunction(func);
        }

        Source = _buffer.ToString();
    }

    public void PrintFunction (IRFunction func) {
        Write($"function '{func.Name}'");

        if (func.Kind == IRFunctionKind.Function) {
            Write(" kind=function");
        }
        else if (func.Kind == IRFunctionKind.Generator) {
            Write(" kind=generator");
        }
        else if (func.Kind == IRFunctionKind.Constructor) {
            Write(" kind=constructor");
        }

        if (func.ReturnType != "Void") {
            Write($" return_type='{func.ReturnType}'");
        }

        Write(" {");

        StartIndent();
        WriteNewLine();

        Write("parameters {");
        StartIndent();

        foreach (var param in func.Parameters) {
            WriteNewLine();
            PrintParameter(param);
        }

        EndIndent();
        WriteNewLine();
        Write("}");

        WriteNewLine();

        PrintStatementList(func.Body);

        EndIndent();
        WriteNewLine();
        Write("}");

        WriteNewLine();
        WriteNewLine();
    }

    public void PrintParameter (IRParameter param) {
        Write($"'{param.Name}' type='{param.Type}' ");
        WriteMutability(param.Mutability);
        Write(";");
    }

    public void PrintStatementList (List<IRStatement> stmtList) {
        foreach (var stmt in stmtList) {
            WriteNewLine();
            PrintStatement(stmt);
        }
    }

    public void PrintStatement (IRStatement stmt) {
        switch (stmt) {
            case IRLocalDeclarationStatement localDeclStmt:
                PrintLocalDeclarationStatement(localDeclStmt);
                break;
            case IRReturnStatement returnStmt:
                PrintReturnStatement(returnStmt);
                break;
            case IRYieldStatement yieldStmt:
                PrintYieldStatement(yieldStmt);
                break;
            case IRExpressionStatement exprStmt:
                PrintExpressionStatement(exprStmt);
                break;
            case IR_P_PrintStatement printStmt:
                Print_P_PrintStatement(printStmt);
                break;
            default:
                throw new NotImplementedException(
                    $"{stmt.GetType().Name} not implemented!"
                );
        }
    }

    public void PrintLocalDeclarationStatement (IRLocalDeclarationStatement stmt) {
        Write($"local '{stmt.Name}' type='{stmt.Type}' ");
        WriteMutability(stmt.Mutability);

        if (stmt.Initialization != null) {
            Write(" init=(");
            PrintExpression(stmt.Initialization);
            Write(")");
        }

        Write(";");
    }

    public void PrintReturnStatement (IRReturnStatement stmt) {
        if (stmt.Expression == null) {
            Write("return;");
        }
        else {
            Write("return (");
            PrintExpression(stmt.Expression);
            Write(");");
        }
    }

    public void PrintYieldStatement (IRYieldStatement stmt) {
        Write("yield (");
        PrintExpression(stmt.Expression);
        Write(");");
    }

    public void PrintExpressionStatement (IRExpressionStatement stmt) {
        PrintExpression(stmt.Expression);
        Write(";");
    }

    public void PrintExpression (IRExpression expr) {
        switch (expr) {
            case IRIfExpression ifExpr:
                WriteNewLine();
                PrintIfExpression(ifExpr);
                break;
            case IRWhileExpression whileExpr:
                WriteNewLine();
                PrintWhileExpression(whileExpr);
                break;
            case IRAssignmentExpression assignmentExpr:
                PrintAssignmentExpression(assignmentExpr);
                break;
            case IRMathBinaryExpression mathBinExpr:
                PrintMathBinaryExpression(mathBinExpr);
                break;
            case IRMathUnaryExpression mathUnaryExpr:
                PrintMathUnaryExpression(mathUnaryExpr);
                break;
            case IRComparisonExpression compExpr:
                PrintComparisonExpression(compExpr);
                break;
            case IRCallExpression callExpr:
                PrintCallExpression(callExpr);
                break;
            case IRIdentifierExpression idExpr:
                PrintIdentifierExpression(idExpr);
                break;
            case IRLiteralExpression litExpr:
                PrintLiteralExpression(litExpr);
                break;
            default:
                throw new NotImplementedException(
                    $"{expr.GetType().Name} not implemented!"
                );
        }
    }

    public void PrintIfExpression (IRIfExpression ifExpr) {
        Write("if test=(");
        PrintExpression(ifExpr.Test);
        Write(") {");

        StartIndent();
        PrintStatementList(ifExpr.Consequent);
        EndIndent();
        WriteNewLine();
        Write("}");

        if (ifExpr.Alternate != null) {
            WriteNewLine();
            Write("else {");

            StartIndent();
            PrintStatementList(ifExpr.Alternate);
            EndIndent();
            Write("}");
        }
    }

    public void PrintWhileExpression (IRWhileExpression whileExpr) {
        Write("while test=(");
        PrintExpression(whileExpr.Test);
        Write(") {");

        StartIndent();
        PrintStatementList(whileExpr.Body);
        EndIndent();

        WriteNewLine();
        Write("}");
    }

    public void PrintAssignmentExpression (IRAssignmentExpression expr) {
        PrintExpression(expr.Left);
        Write($" = ");
        PrintExpression(expr.Right);
    }

    public void PrintMathBinaryExpression (IRMathBinaryExpression expr) {
        WriteMathOperator(expr.Operation);
        Write("(");
        PrintExpression(expr.Left);
        Write(", ");
        PrintExpression(expr.Right);
        Write(")");
    }

    public void PrintMathUnaryExpression (IRMathUnaryExpression expr) {
        WriteUnaryOperator(expr.Operation);
        Write("(");
        PrintExpression(expr.Expression);
        Write(")");
    }

    public void PrintComparisonExpression (IRComparisonExpression expr) {
        WriteComparisonOperator(expr.Operation);
        Write("(");
        PrintExpression(expr.Left);
        Write(", ");
        PrintExpression(expr.Right);
        Write(")");
    }

    public void PrintCallExpression (IRCallExpression expr) {
        PrintExpression(expr.Callee);

        Write("(");

        for (int i = 0; i < expr.Arguments.Count; i++) {
            PrintArgument(expr.Arguments[i]);
            if (i != expr.Arguments.Count - 1) {
                Write(", ");
            }
        }

        Write(")");
    }

    public void PrintArgument (IRArgument arg) {
        PrintExpression(arg.Expression);
    }

    public void PrintIdentifierExpression (IRIdentifierExpression expr) {
        switch (expr.Kind) {
            case IRIdentifierKind.Local:
                Write($"'{expr.Name}'");
                break;
            case IRIdentifierKind.Global:
                Write($":'{expr.Name}'");
                break;
        }
    }

    public void PrintLiteralExpression (IRLiteralExpression expr) {
        switch (expr.Value.Kind) {
            case ConstantValueKind.Integer:
                Write($"{expr.Value.AsInteger}");
                break;
            case ConstantValueKind.UnsignedInteger:
                Write($"{expr.Value.AsUnsignedInteger}");
                break;
            case ConstantValueKind.Float:
                Write($"{expr.Value.AsFloat}");
                break;
            case ConstantValueKind.Boolean:
                Write($"{expr.Value.AsBoolean}");
                break;
            case ConstantValueKind.String:
                Write($"\"{expr.Value.AsString}\"");
                break;
        }
    }

    public void Print_P_PrintStatement (IR_P_PrintStatement stmt) {
        Write("__p_print(");
        PrintExpression(stmt.Expression);
        Write(")");
    }

    private void WriteMutability (IRMutability mutability) {
        switch (mutability) {
            case IRMutability.Constant:
                Write("constant");
                break;
            case IRMutability.Variable:
                Write("variable");
                break;
        }
    }

    private void WriteMathOperator (IRMathOperation op) {
        switch (op) {
            case IRMathOperation.Add:
                Write("+");
                break;
            case IRMathOperation.Subtract:
                Write("-");
                break;
            case IRMathOperation.Multiply:
                Write("*");
                break;
            case IRMathOperation.Divide:
                Write("/");
                break;
        }
    }

    private void WriteUnaryOperator (IRUnaryOperation op) {
        switch (op) {
            case IRUnaryOperation.Negate:
                Write("-");
                break;
        }
    }

    private void WriteComparisonOperator (IRComparisonOperation op) {
        switch (op) {
            case IRComparisonOperation.Equals:
                Write("==");
                break;
            case IRComparisonOperation.NotEquals:
                Write("!=");
                break;
            case IRComparisonOperation.LessThan:
                Write("<");
                break;
            case IRComparisonOperation.LessThanOrEqualTo:
                Write("<=");
                break;
            case IRComparisonOperation.GreaterThan:
                Write(">");
                break;
            case IRComparisonOperation.GreaterThanOrEqualTo:
                Write(">=");
                break;
        }
    }

    private void Write (string text) {
        _buffer.Append(text);
    }

    private void WriteNewLine () {
        _buffer.Append("\n" + new string(' ', _indentation * SpacesPerIndent));
    }

    private void RemoveIndent () {
        _buffer.Remove(_buffer.Length - SpacesPerIndent, SpacesPerIndent);
    }

    private void StartIndent () {
        _indentation++;
    }

    private void EndIndent () {
        _indentation--;
        if (_indentation < 0) {
            throw new("Impossible indentation level.");
        }
    }
}
