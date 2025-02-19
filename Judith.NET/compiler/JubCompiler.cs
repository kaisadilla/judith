using Judith.NET.analysis.binder;
using Judith.NET.analysis.syntax;
using Judith.NET.compiler.jub;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Judith.NET.compiler;

public class JubCompiler : SyntaxVisitor {
    const int MAX_LOCALS = ushort.MaxValue + 1;

    private Compilation _cmp;
    private BinaryFunction? _currentFunc = null;
    private LocalManager? _localManager = null;

    public BinaryFile Bin { get; private init; } = new();

    public JubCompiler (Compilation cmp) {
        _cmp = cmp;
    }

    public void Compile () {
        _currentFunc = new();
        _localManager = new(MAX_LOCALS);

        Visit(_cmp.Units[0]);

        Bin.Functions.Add(_currentFunc);
        Bin.EntryPoint = Bin.Functions.Count - 1;
        _currentFunc = null;
        _localManager = null;
    }
    
    private void BeginScope () {
        RequireFunction();

        _localManager.ScopeDepth++;
    }

    private void EndScope () {
        RequireFunction();

        _localManager.ScopeDepth--;
    }

    public override void Visit (CompilerUnit node) {
        if (node.ImplicitFunction != null) Visit(node.ImplicitFunction);
    }

    public override void Visit (LocalDeclarationStatement node) {
        RequireFunction();

        if (node.DeclaratorList.Declarators.Count != 1) {
            throw new NotImplementedException("Multiple local declaration not yet implemented.");
        }

        string localName = node.DeclaratorList.Declarators[0].Identifier.Name;
        // Check if a local with this name is already declared.
        if (_localManager.IsLocalDeclared(localName)) {
            throw new NotImplementedException("Local shadowing not yet implemented.");
        }

        int addr = _localManager.AddLocal(localName);

        if (node.Initializer == null) return;

        Visit(node.Initializer);
        EmitStore(addr, node.Initializer.Line);
        _localManager.MarkInitialized(addr);
    }

    public override void Visit (IfExpression node) {
        Visit(node.Test);

        var thenJump = EmitJump(OpCode.JFalse, node.Test.Line);
        Visit(node.Consequent);

        int elseJump = -1;
        if (node.Alternate != null) {
            elseJump = EmitJump(OpCode.Jmp, node.Test.Line);
        }

        PatchJump(thenJump);

        if (node.Alternate != null) {
            Visit(node.Alternate);
            PatchJump(elseJump);
        }
    }

    public override void Visit (LiteralExpression node) {
        RequireFunction();
    
        if (_cmp.Binder.TryGetBoundNode(node, out BoundLiteralExpression? boundNode) == false) {
            ThrowUnboundNode(node);
        }

        // TODO: When type alias desugaring is added, this compares directly to
        // F64, F32, I64, I32, etc.
        if (boundNode.Type == _cmp.Native.Types.Num) {
            int index = Bin.ConstantTable.WriteFloat64(boundNode.Value.AsFloat);

            if (index <= byte.MaxValue) {
                _currentFunc.Chunk.WriteInstruction(OpCode.Const, node.Line);
                _currentFunc.Chunk.WriteByte((byte)index, node.Line);
            }
            else {
                _currentFunc.Chunk.WriteInstruction(OpCode.ConstLong, node.Line);
                _currentFunc.Chunk.WriteInt32(index, node.Line);
            }
        }
        else if (boundNode.Type == _cmp.Native.Types.Bool) {
            if (boundNode.Value.AsBoolean == true) {
                _currentFunc.Chunk.WriteInstruction(OpCode.IConst1, node.Line);
            }
            else {
                _currentFunc.Chunk.WriteInstruction(OpCode.Const0, node.Line);
            }
        }
        else if (boundNode.Type == _cmp.Native.Types.String) {
            int index = Bin.ConstantTable.WriteStringASCII(boundNode.Value.AsString!);

            if (index <= byte.MaxValue) {
                _currentFunc.Chunk.WriteInstruction(OpCode.ConstStr, node.Line);
                _currentFunc.Chunk.WriteByte((byte)index, node.Line);
            }
            else {
                _currentFunc.Chunk.WriteInstruction(OpCode.ConstStrLong, node.Line);
                _currentFunc.Chunk.WriteInt32(index, node.Line);
            }
        }
    }

    public override void Visit (IdentifierExpression node) {
        RequireFunction();

        if (_localManager.TryGetLocalAddr(node.Identifier.Name, out int addr) == false) {
            throw new Exception("Local not found");
        }

        EmitLoad(addr, node.Line);
    }

    public override void Visit (ReturnStatement node) {
        RequireFunction();
        // TODO: Compile expression.

        _currentFunc.Chunk.WriteInstruction(OpCode.Ret, node.Line);
    }

    public override void Visit (BinaryExpression node) {
        if (node.Kind == SyntaxKind.LogicalBinaryExpression) {
            if (node.Operator.OperatorKind == OperatorKind.LogicalAnd) {
                CompileAndBinaryExpression(node);
            }
            else {
                CompileOrBinaryExpression(node);
            }
        }
        else {
            CompileRegularBinaryExpression(node);
        }
    }

    public override void Visit (AssignmentExpression node) {
        RequireFunction();

        if (node.Left.Kind != SyntaxKind.IdentifierExpression) {
            throw new Exception("Can't assign to that."); // TODO: Compile error.
        }
        if (node.Left is not IdentifierExpression idExpr) {
            throw new Exception("Invalid type.");
        }

        Visit(node.Right);

        if (_localManager.TryGetLocalAddr(idExpr.Identifier.Name, out int addr) == false) {
            throw new Exception("Local not found");
        }

        EmitStore(addr, node.Line);
    }

    public override void Visit (EqualsValueClause node) {
        Visit(node.Value);
    }

    public override void Visit (P_PrintStatement node) {
        RequireFunction();

        Visit(node.Expression);

        if (_cmp.Binder.TryGetBoundNode(node.Expression, out BoundExpression? boundExpr) == false) {
            ThrowUnboundNode(node.Expression);
        }

        _currentFunc.Chunk.WriteInstruction(OpCode.Print, node.Line);

        if (boundExpr.Type == _cmp.Native.Types.Num) {
            _currentFunc.Chunk.WriteByte((byte)ConstantType.Float64, node.Expression.Line);
        }
        else if (boundExpr.Type == _cmp.Native.Types.String) {
            _currentFunc.Chunk.WriteByte((byte)ConstantType.StringASCII, node.Expression.Line);
        }
        else {
            _currentFunc.Chunk.WriteByte((byte)ConstantType.Bool, node.Expression.Line);
        }
    }

    private void CompileAndBinaryExpression (BinaryExpression node) {
        RequireFunction();

        // When an expression evaluates to false, it'll store a JFalseK here
        // that will jump to the "set to false" line.
        List<int> jumpsToPatch = new();

        CompileNestedOrBinaryExpression(node, OperatorKind.LogicalAnd, jumpsToPatch);
        PatchJumps(jumpsToPatch); // All failed expressions jump to set to false.
    }

    private void CompileOrBinaryExpression (BinaryExpression node) {
        RequireFunction();

        // When an expression evaluates to true, it'll store a JTrueK here
        // that will jump to after the "set to false" line.
        List<int> jumpsToPatch = new();

        CompileNestedOrBinaryExpression(node, OperatorKind.LogicalOr, jumpsToPatch);

        PatchJumps(jumpsToPatch); // All JTrueK jump to here, to skip set to false.
    }

    private void CompileNestedOrBinaryExpression (
        BinaryExpression expr, OperatorKind op, List<int> jumpList
    ) {
        if (expr.Left.Kind == SyntaxKind.LogicalBinaryExpression) {
            var binaryExpr = CastOrThrow<BinaryExpression>(expr.Left);
            if (binaryExpr.Operator.OperatorKind == op) {
                CompileNestedOrBinaryExpression(binaryExpr, op, jumpList);
            }
            else {
                Visit(expr.Left);
            }
        }
        else {
            Visit(expr.Left);
        }

        if (op == OperatorKind.LogicalAnd) {
            jumpList.Add(EmitJump(OpCode.JFalseK, expr.Line));
        }
        else { // OperatorKind.LogicalOr
            jumpList.Add(EmitJump(OpCode.JTrueK, expr.Line));
        }

        Visit(expr.Right);
    }

    private void CompileRegularBinaryExpression (BinaryExpression node) {
        RequireFunction();

        Visit(node.Left);
        Visit(node.Right);

        switch (node.Operator.OperatorKind) {
            case OperatorKind.Add:
                _Instr(OpCode.FAdd);
                return;
            case OperatorKind.Subtract:
                _Instr(OpCode.FSub);
                return;
            case OperatorKind.Multiply:
                _Instr(OpCode.FMul);
                return;
            case OperatorKind.Divide:
                _Instr(OpCode.FDiv);
                return;
            case OperatorKind.Equals:
                _Instr(OpCode.Eq);
                return;
            case OperatorKind.NotEquals:
                _Instr(OpCode.Neq);
                return;
            case OperatorKind.LessThan:
                _Instr(OpCode.FLt);
                return;
            case OperatorKind.LessThanOrEqualTo:
                _Instr(OpCode.FLe);
                return;
            case OperatorKind.GreaterThan:
                _Instr(OpCode.FGt);
                return;
            case OperatorKind.GreaterThanOrEqualTo:
                _Instr(OpCode.FGe);
                return;
            case OperatorKind.LogicalAnd:
                break;
            case OperatorKind.LogicalOr:
                break;
        }


        throw new NotImplementedException(
            $"Unimplemented operator: '{node.Operator.OperatorKind}'."
        );

        void _Instr (OpCode opCode) {
            _currentFunc.Chunk.WriteInstruction(opCode, node.Operator.Line);
        }
    }

    private T CastOrThrow<T> (SyntaxNode node) where T : SyntaxNode {
        if (node is not T tNode) {
            throw new Exception("Class and SyntaxKind mismatch.");
        }

        return tNode;
    }

    /// <summary>
    /// Writes the STORE instruction required for the address given.
    /// </summary>
    /// <param name="addr">The address of the variable in the local variable array.</param>
    /// <param name="line">The line of code that produced this.</param>
    private void EmitStore (int addr, int line) {
        RequireFunction();

        if (addr == 0) {
            _currentFunc.Chunk.WriteInstruction(OpCode.Store0, line);
        }
        else if (addr == 1) {
            _currentFunc.Chunk.WriteInstruction(OpCode.Store1, line);
        }
        else if (addr == 2) {
            _currentFunc.Chunk.WriteInstruction(OpCode.Store2, line);
        }
        else if (addr == 3) {
            _currentFunc.Chunk.WriteInstruction(OpCode.Store3, line);
        }
        else if (addr == 4) {
            _currentFunc.Chunk.WriteInstruction(OpCode.Store4, line);
        }
        else if (addr <= byte.MaxValue) {
            _currentFunc.Chunk.WriteInstruction(OpCode.Store, line);
            _currentFunc.Chunk.WriteByte((byte)addr, line);
        }
        else {
            throw new NotImplementedException("VM does not yet support locals beyond 255");
            //Chunk.WriteInstruction(OpCode.StoreLong, line);
            //Chunk.WriteUint16((ushort)addr, line);
        }
    }

    /// <summary>
    /// Writes the LOAD instruction required for the address given.
    /// </summary>
    /// <param name="addr">The address of the variable in the local variable array.</param>
    /// <param name="line">The line of code that produced this.</param>
    private void EmitLoad (int addr, int line) {
        RequireFunction();

        if (addr == 0) {
            _currentFunc.Chunk.WriteInstruction(OpCode.Load0, line);
        }
        else if (addr == 1) {
            _currentFunc.Chunk.WriteInstruction(OpCode.Load1, line);
        }
        else if (addr == 2) {
            _currentFunc.Chunk.WriteInstruction(OpCode.Load2, line);
        }
        else if (addr == 3) {
            _currentFunc.Chunk.WriteInstruction(OpCode.Load3, line);
        }
        else if (addr == 4) {
            _currentFunc.Chunk.WriteInstruction(OpCode.Load4, line);
        }
        else if (addr <= byte.MaxValue) {
            _currentFunc.Chunk.WriteInstruction(OpCode.Load, line);
            _currentFunc.Chunk.WriteByte((byte)addr, line);
        }
        else {
            throw new NotImplementedException("VM does not yet support locals beyond 255");
            //Chunk.WriteInstruction(OpCode.LoadLong, line);
            //Chunk.WriteUint16((ushort)addr, line);
        }
    }

    /// <summary>
    /// Emits the short jump code given and a byte for the offset set at 0.
    /// Returns the index of the offset byte so it can be patched with PatchJump.
    /// </summary>
    /// <param name="code">The opcode to emit.</param>
    /// <param name="line">The line that caused this jump.</param>
    /// <returns></returns>
    private int EmitJump (OpCode code, int line) {
        RequireFunction();

        _currentFunc.Chunk.WriteInstruction(code, line);
        _currentFunc.Chunk.WriteByte(0, line);

        return _currentFunc.Chunk.Code.Count - 1;
    }

    /// <summary>
    /// Patches the jump byte at the offset given so it points to the current
    /// instruction.
    /// </summary>
    /// <param name="offsetByte">The byte that stores the jump offset.</param>
    private void PatchJump (int offsetByte) {
        RequireFunction();

        int jumpOffset = _currentFunc.Chunk.Code.Count - offsetByte - 1;

        if (jumpOffset < sbyte.MinValue || jumpOffset > sbyte.MaxValue) {
            throw new NotImplementedException("Long jumps are not implemented");
        }

        _currentFunc.Chunk.Code[offsetByte] = (byte)((sbyte)jumpOffset);
    }

    private void PatchJumps (IEnumerable<int> offsets) {
        foreach (var o in offsets) PatchJump(o);
    }

    #region Requires
    /// <summary>
    /// Checks that the current context is that of a function, asserting that
    /// _currentFunction and _localManager exist.
    /// </summary>
    /// <exception cref="Exception"></exception>
    [MemberNotNull(nameof(_currentFunc))]
    [MemberNotNull(nameof(_localManager))]
    private void RequireFunction () {
        if (_currentFunc == null || _localManager == null) {
            throw new Exception(
                "This node can only be compiled in the context of a function."
            );
        }
    }

    [DoesNotReturn]
    private void ThrowUnboundNode (SyntaxNode node) {
        throw new Exception($"Cannot compile incomplete bound node '{node}'");
    }
    #endregion
}
