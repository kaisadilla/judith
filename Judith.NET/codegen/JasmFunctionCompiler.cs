using Judith.NET.codegen.jasm;
using Judith.NET.ir;
using Judith.NET.ir.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.codegen;

public class JasmFunctionCompiler {
    const int MAX_LOCALS = ushort.MaxValue + 1;

    private JasmGenerator _generator;
    private JasmBlock _block;
    private IRFunction _irFunc;

    private LocalBlock _localBlock;

    public JasmFunction JasmFunction { get; private set; }

    private Chunk Chunk => JasmFunction.Chunk;

    private IRNativeHeader.TypeCollection NativeTypes
        => _generator.Program.NativeHeader.TypeRefs;

    public JasmFunctionCompiler (JasmGenerator generator, JasmBlock block, IRFunction irFunc) {
        _generator = generator;
        _block = block;
        _irFunc = irFunc;

        _localBlock = new(MAX_LOCALS);

        int nameIndex = _block.StringTable.GetStringIndex(irFunc.Name);

        JasmFunction = new(nameIndex);
    }

    public void CompileFunction () {
        CompileParameters(_irFunc.Parameters);
        CompileBody(_irFunc.Body);

        JasmFunction.MaxLocals = _localBlock.MaxLocals;
    }

    public void CompileParameters (List<IRParameter> parameters) {
        // For each parameter:
        foreach (var param in parameters) {
            // Add the parameter as a normal local in the local block.
            int addr = _localBlock.AddLocal(param.Name);
            _localBlock.MarkInitialized(addr);

            int nameIndex = _block.StringTable.GetStringIndex(param.Name);

            // Get the type reference in the assembly for this parameter's type.
            int typeRefIndex = _generator.GetTypeReferenceIndex(param.Type);

            // Add the parameter itself to the Jasm Function's metadata.
            JasmFunction.Parameters.Add(new(nameIndex, typeRefIndex));
        }

        // We load parameter values from last to first, as the last argument
        // is at the top of the stack.
        for (int i = parameters.Count - 1; i >= 0; i--) {
            Chunk.WriteStore(i);
        }
    }

    public void CompileBody (List<IRStatement> statements) {
        foreach (var stmt in statements) {
            CompileStatement(stmt);
        }
    }

    public void CompileStatement (IRStatement statement) {
        switch (statement) {
            case IRLocalDeclarationStatement localDeclStmt:
                CompileLocalDeclarationStatement(localDeclStmt);
                break;
            case IRReturnStatement returnStmt:
                CompileReturnStatement(returnStmt);
                break;
            case IRYieldStatement returnStmt:
                CompileYieldStatement(returnStmt);
                break;
            case IRExpressionStatement exprStmt:
                CompileExpressionStatement(exprStmt);
                break;
            case IR_P_PrintStatement printStmt:
                Compile_P_PrintStatement(printStmt);
                break;
            default:
                throw new NotImplementedException(
                    $"{statement.GetType().Name} not implemented!"
                );
        }
    }

    public void CompileLocalDeclarationStatement (IRLocalDeclarationStatement stmt) {
        // Shadowing locals is not allowed in IR.
        if (_localBlock.IsLocalDeclared(stmt.Name)) throw new InvalidIRProgramException(
            "A function cannot contain two locals with the same name."
        );

        // Get the address of the local inside the function.
        int addr = _localBlock.AddLocal(stmt.Name);

        if (stmt.Initialization == null) return;

        // If the declaration initializes the value, compile the initialization
        // expression and store it in the local's address.
        CompileExpression(stmt.Initialization);
        Chunk.WriteStore(addr);
        
        // We can now mark the local as initialized.
        _localBlock.MarkInitialized(addr);
    }

    public void CompileReturnStatement (IRReturnStatement stmt) {
        if (stmt.Expression != null) {
            CompileExpression(stmt.Expression);
        }

        Chunk.WriteInstruction(OpCode.RET);
    }

    public void CompileYieldStatement (IRYieldStatement stmt) {
        CompileExpression(stmt.Expression);
    }

    public void CompileExpressionStatement (IRExpressionStatement stmt) {
        CompileExpression(stmt.Expression);

        // When the expression returns a value, the value will not be used, so
        // we immediately pop it off the stack.
        if (stmt.Expression.Type != _generator.Program.NativeHeader.TypeRefs.Void.Name) {
            Chunk.WriteInstruction(OpCode.POP);
        }
    }

    public void CompileExpression (IRExpression expr) {
        switch (expr) {
            case IRIfExpression ifExpr:
                CompileIfExpression(ifExpr);
                break;
            case IRWhileExpression whileExpr:
                CompileWhileExpression(whileExpr);
                break;
            case IRAssignmentExpression assignmentExpr:
                CompileAssignmentExpression(assignmentExpr);
                break;
            case IRMathBinaryExpression mathBinExpr:
                CompileMathBinaryExpression(mathBinExpr);
                break;
            case IRMathUnaryExpression mathUnaryExpr:
                CompileMathUnaryExpression(mathUnaryExpr);
                break;
            case IRComparisonExpression compExpr:
                CompileComparisonExpression(compExpr);
                break;
            case IRCallExpression callExpr:
                CompileCallExpression(callExpr);
                break;
            case IRIdentifierExpression idExpr:
                CompileIdentifierExpression(idExpr);
                break;
            case IRLiteralExpression litExpr:
                CompileLiteralExpression(litExpr);
                break;
            default:
                throw new NotImplementedException(
                    $"{expr.GetType().Name} not implemented!"
                );
        }
    }

    public void CompileIfExpression (IRIfExpression expr) {
        // Check condition
        CompileExpression(expr.Test);

        // Prepare a jump to skip the "then" block if test fails.
        var elseJump = Chunk.WriteJump(OpCode.JFALSE);

        // Compile the "then" body.
        _localBlock.BeginScope();
        CompileBody(expr.Consequent);
        _localBlock.EndScope();

        // Prepare a jump to skip the "else" body if it exists.
        int skipElseJump = -1;
        if (expr.Alternate != null) {
            skipElseJump = Chunk.WriteJump(OpCode.JMP);
        }

        // Direct the "else" jump to here.
        Chunk.PatchJump(elseJump);

        // If there's an "else" block, the "else" block goes here..
        if (expr.Alternate != null) {
            // Compile the "else" body.
            _localBlock.BeginScope();
            CompileBody(expr.Alternate);
            _localBlock.EndScope();

            // Direct the jump that skips the else block to here.
            Chunk.PatchJump(skipElseJump);
        }
    }

    public void CompileWhileExpression (IRWhileExpression expr) {
        // Store where this loop ends.
        var loopStart = Chunk.Index;
        // Check condition
        CompileExpression(expr.Test);
        // Prepare a jump to skip the body if the test fails.
        var falseJump = Chunk.WriteJump(OpCode.JFALSE);

        // Compile the body
        _localBlock.BeginScope();
        CompileBody(expr.Body);
        _localBlock.EndScope();

        // Emit a jump back to the start of the loop.
        Chunk.WriteJumpBack(OpCode.JMP, loopStart);
        // Direct the skip body jump to here.
        Chunk.PatchJump(falseJump);
    }

    public void CompileAssignmentExpression (IRAssignmentExpression expr) {
        if (expr.Left is not IRIdentifierExpression leftIdExpr) {
            throw new NotImplementedException(
                "Cannot yet assign to anything other than an id."
            );
        }
        if (leftIdExpr.Kind != IRIdentifierKind.Local) {
            throw new NotImplementedException(
                "Cannot yet assign to globals."
            );
        }

        if (_localBlock.TryGetLocalAddr(leftIdExpr.Name, out int addr) == false) {
            throw new InvalidIRProgramException(
                $"Local '{leftIdExpr.Name}' does not exist in the current context."
            );
        }

        CompileExpression(expr.Right);
        Chunk.WriteStore(addr);
        Chunk.WriteLoad(addr);
    }

    public void CompileMathBinaryExpression (IRMathBinaryExpression expr) {
        if (expr.Left.Type != expr.Right.Type) throw new InvalidIRProgramException(
            "Comparisons in JIR can only be made between values of the same type."
        );

        CompileExpression(expr.Left);
        CompileExpression(expr.Right);

        var irType = _generator.Resolver.GetIRType(expr.Left.Type);

        if (irType == NativeTypes.F64) {
            Chunk.WriteInstruction(expr.Operation switch {
                IRMathOperation.Add => OpCode.F_ADD,
                IRMathOperation.Subtract => OpCode.F_SUB,
                IRMathOperation.Multiply => OpCode.F_MUL,
                IRMathOperation.Divide => OpCode.F_DIV,
                _ => throw new InvalidIRProgramException("Invalid comparison operation."),
            });
        }
        else if (irType == NativeTypes.I64) {
            Chunk.WriteInstruction(expr.Operation switch {
                IRMathOperation.Add => OpCode.I_ADD,
                IRMathOperation.Subtract => OpCode.I_SUB,
                IRMathOperation.Multiply => OpCode.I_MUL,
                IRMathOperation.Divide => OpCode.I_DIV,
                _ => throw new InvalidIRProgramException("Invalid comparison operation."),
            });
        }
        else {
            throw new NotImplementedException("Other comparisons not yet implemented.");
        }
    }

    public void CompileMathUnaryExpression (IRMathUnaryExpression expr) {
        CompileExpression(expr.Expression);

        var irType = _generator.Resolver.GetIRType(expr.Type);

        if (irType == NativeTypes.F64) {
            Chunk.WriteInstruction(expr.Operation switch {
                IRUnaryOperation.Negate => OpCode.F_NEG,
                _ => throw new InvalidIRProgramException("Invalid unary operation."),
            });
        }
        else if (irType == NativeTypes.I64) {
            Chunk.WriteInstruction(expr.Operation switch {
                IRUnaryOperation.Negate => OpCode.I_NEG,
                _ => throw new InvalidIRProgramException("Invalid unary operation."),
            });
        }
        else {
            throw new NotImplementedException("Other unary operations not yet implemented.");
        }
    }

    public void CompileComparisonExpression (IRComparisonExpression expr) {
        if (expr.Left.Type != expr.Right.Type) throw new InvalidIRProgramException(
            "Comparisons in JIR can only be made between values of the same type."
        );

        CompileExpression(expr.Left);
        CompileExpression(expr.Right);

        var irType = _generator.Resolver.GetIRType(expr.Left.Type);

        if (irType == NativeTypes.F64) {
            Chunk.WriteInstruction(expr.Operation switch {
                IRComparisonOperation.Equals => OpCode.EQ,
                IRComparisonOperation.NotEquals => OpCode.NEQ,
                IRComparisonOperation.LessThan => OpCode.F_LT,
                IRComparisonOperation.LessThanOrEqualTo => OpCode.F_LE,
                IRComparisonOperation.GreaterThan => OpCode.F_GT,
                IRComparisonOperation.GreaterThanOrEqualTo => OpCode.F_GE,
                _ => throw new InvalidIRProgramException("Invalid comparison operation."),
            });
        }
        else if (irType == NativeTypes.I64) {
            Chunk.WriteInstruction(expr.Operation switch {
                IRComparisonOperation.Equals => OpCode.EQ,
                IRComparisonOperation.NotEquals => OpCode.NEQ,
                IRComparisonOperation.LessThan => OpCode.I_LT,
                IRComparisonOperation.LessThanOrEqualTo => OpCode.I_LE,
                IRComparisonOperation.GreaterThan => OpCode.I_GT,
                IRComparisonOperation.GreaterThanOrEqualTo => OpCode.I_GE,
                _ => throw new InvalidIRProgramException("Invalid comparison operation."),
            });
        }
        else {
            throw new NotImplementedException("Other comparisons not yet implemented.");
        }
    }

    public void CompileCallExpression (IRCallExpression expr) {
        foreach (var arg in expr.Arguments) {
            CompileArgument(arg);
        }

        if (expr.Callee is not IRIdentifierExpression idCallee) {
            throw new NotImplementedException("Cannot yet call results of expressions.");
        }
        if (idCallee.Kind != IRIdentifierKind.Global) {
            throw new NotImplementedException("Cannot yet call results of expressions.");
        }
        if (_generator.Resolver.TryGetIRFunction(idCallee.Name, out _)) {
            var funcRefIndex = _generator.GetFunctionReferenceIndex(idCallee.Name);
            Chunk.WriteCall(funcRefIndex);
        }
    }

    public void CompileArgument (IRArgument argument) {
        CompileExpression(argument.Expression);
    }

    public void CompileIdentifierExpression (IRIdentifierExpression expr) {
        if (expr.Kind == IRIdentifierKind.Local) {
            if (_localBlock.TryGetLocalAddr(expr.Name, out int addr) == false) {
                throw new InvalidIRProgramException(
                    $"Local '{expr.Name}' does not exist in the current context."
                );
            }

            Chunk.WriteLoad(addr);
        }
        else if (expr.Kind == IRIdentifierKind.Global) {
            throw new InvalidIRProgramException("Cannot yet use global names.");
        }
    }

    public void CompileLiteralExpression (IRLiteralExpression expr) {
        var irType = _generator.Resolver.GetIRType(expr.Type);

        if (irType == NativeTypes.F64) {
            Chunk.WriteF64Const(expr.Value.AsFloat);
        }
        else if (irType == NativeTypes.Bool) {
            Chunk.WriteBoolConst(expr.Value.AsBoolean);
        }
        else if (irType == NativeTypes.String) {
            int index = _block.StringTable.GetStringIndex(expr.Value.AsString!);

            Chunk.WriteUtf8StringConst(index);
        }
        else {
            throw new NotImplementedException("Can't compile value of this type yet.");
        }
    }

    public void Compile_P_PrintStatement (IR_P_PrintStatement stmt) {
        CompileExpression(stmt.Expression);

        Chunk.WriteInstruction(OpCode.PRINT);

        var irType = _generator.Resolver.GetIRType(stmt.Expression.Type);

        if (irType == NativeTypes.F64) {
            Chunk.WriteByte((byte)JasmConstantType.Float64);
        }
        else if (irType == NativeTypes.String) {
            Chunk.WriteByte((byte)JasmConstantType.StringUtf8);
        }
        else {
            Chunk.WriteByte((byte)JasmConstantType.Bool);
        }
    }
}
