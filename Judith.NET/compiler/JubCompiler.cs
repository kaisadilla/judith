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
        WriteStore(addr, node.Initializer.Line);
        _localManager.MarkInitialized(addr);
    }

    public override void Visit (EqualsValueClause node) {
        Visit(node.Value);
    }

    public override void Visit (LiteralExpression node) {
        RequireFunction();
    
        if (_cmp.Binder.TryGetBoundNode(node, out BoundLiteralExpression? boundNode) == false) {
            throw new Exception($"Cannot compile incomplete bound node '{node}'");
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

        WriteLoad(addr, node.Line);
    }

    public override void Visit (ReturnStatement node) {
        RequireFunction();
        // TODO: Compile expression.

        _currentFunc.Chunk.WriteInstruction(OpCode.Ret, node.Line);
    }

    public override void Visit (BinaryExpression node) {
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
                break;
            case OperatorKind.NotEquals:
                break;
            case OperatorKind.LessThan:
                break;
            case OperatorKind.LessThanOrEqualTo:
                break;
            case OperatorKind.GreaterThan:
                break;
            case OperatorKind.GreaterThanOrEqualTo:
                break;
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

        WriteStore(addr, node.Line);
    }

    public override void Visit (P_PrintStatement node) {
        RequireFunction();

        Visit(node.Expression);

        _currentFunc.Chunk.WriteInstruction(OpCode.Print, node.Line);
    }

    /// <summary>
    /// Writes the STORE instruction required for the address given.
    /// </summary>
    /// <param name="addr">The address of the variable in the local variable array.</param>
    /// <param name="line">The line of code that produced this.</param>
    private void WriteStore (int addr, int line) {
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
    private void WriteLoad (int addr, int line) {
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
    #endregion
}
