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

    private List<SyntaxNode> _ast;
    private BinaryFunction? _currentFunction = null;
    private LocalManager? _localManager = null;

    public BinaryFile Bin { get; private init; } = new();

    public JubCompiler (List<SyntaxNode> ast) {
        _ast = ast;
    }

    public void Compile () {
        _currentFunction = new();
        _localManager = new(MAX_LOCALS);

        Visit(_ast);

        Bin.Functions.Add(_currentFunction);
        Bin.EntryPoint = Bin.Functions.Count - 1;
        _currentFunction = null;
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

        int index;
        // TODO and WARNING: LiteralKind is the type of literal the user wrote,
        // not the actual type of the value it represents. I.e. any number the
        // user writes (without an suffix) is considered an Int64 if it doesn't
        // have a decimal point or a Float64 if it does. in "const a: Float = 3",
        // that 3 is parsed as an Int64 number. In the future, the type resolution
        // pass will identify the type each number should have.
        if (node.Literal.LiteralKind == LiteralKind.Float64) {
            if (node.Literal.Value is FloatValue fval) {
                index = Bin.ConstantTable.WriteFloat64(fval.Value);
            }
            else {
                throw new Exception("Literal node (F64) has invalid value.");
            }
        }
        else if (node.Literal.LiteralKind == LiteralKind.Int64) {
            if (node.Literal.Value is IntegerValue ival) {
                index = Bin.ConstantTable.WriteFloat64((double)ival.Value);
            }
            else {
                throw new Exception("Literal node (I64) has invalid value.");
            }
        }
        else if (node.Literal.LiteralKind == LiteralKind.String) {
            if (node.Literal.Value is StringValue sval) {
                index = Bin.ConstantTable.WriteStringASCII(sval.Value);
            }
            else {
                throw new Exception("Literal node (String) has invalid value.");
            }
        }
        else {
            throw new NotImplementedException("Literals of this type cannot yet be added to the constant stack.");
        }

        if (node.Literal.LiteralKind == LiteralKind.String) {
            if (index < byte.MaxValue + 1) {
                _currentFunction.Chunk.WriteInstruction(OpCode.ConstStr, node.Line);
                _currentFunction.Chunk.WriteByte((byte)index, node.Line);
            }
            else {
                _currentFunction.Chunk.WriteInstruction(OpCode.ConstStrLong, node.Line);
                _currentFunction.Chunk.WriteInt32(index, node.Line);
            }
        }
        else {
            if (index < byte.MaxValue + 1) {
                _currentFunction.Chunk.WriteInstruction(OpCode.Const, node.Line);
                _currentFunction.Chunk.WriteByte((byte)index, node.Line);
            }
            else {
                _currentFunction.Chunk.WriteInstruction(OpCode.ConstLong, node.Line);
                _currentFunction.Chunk.WriteInt32(index, node.Line);
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

        _currentFunction.Chunk.WriteInstruction(OpCode.Ret, node.Line);
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
            _currentFunction.Chunk.WriteInstruction(opCode, node.Operator.Line);
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

        _currentFunction.Chunk.WriteInstruction(OpCode.Print, node.Line);
    }

    /// <summary>
    /// Writes the STORE instruction required for the address given.
    /// </summary>
    /// <param name="addr">The address of the variable in the local variable array.</param>
    /// <param name="line">The line of code that produced this.</param>
    private void WriteStore (int addr, int line) {
        RequireFunction();

        if (addr == 0) {
            _currentFunction.Chunk.WriteInstruction(OpCode.Store0, line);
        }
        else if (addr == 1) {
            _currentFunction.Chunk.WriteInstruction(OpCode.Store1, line);
        }
        else if (addr == 2) {
            _currentFunction.Chunk.WriteInstruction(OpCode.Store2, line);
        }
        else if (addr == 3) {
            _currentFunction.Chunk.WriteInstruction(OpCode.Store3, line);
        }
        else if (addr == 4) {
            _currentFunction.Chunk.WriteInstruction(OpCode.Store4, line);
        }
        else if (addr <= byte.MaxValue) {
            _currentFunction.Chunk.WriteInstruction(OpCode.Store, line);
            _currentFunction.Chunk.WriteByte((byte)addr, line);
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
            _currentFunction.Chunk.WriteInstruction(OpCode.Load0, line);
        }
        else if (addr == 1) {
            _currentFunction.Chunk.WriteInstruction(OpCode.Load1, line);
        }
        else if (addr == 2) {
            _currentFunction.Chunk.WriteInstruction(OpCode.Load2, line);
        }
        else if (addr == 3) {
            _currentFunction.Chunk.WriteInstruction(OpCode.Load3, line);
        }
        else if (addr == 4) {
            _currentFunction.Chunk.WriteInstruction(OpCode.Load4, line);
        }
        else if (addr <= byte.MaxValue) {
            _currentFunction.Chunk.WriteInstruction(OpCode.Load, line);
            _currentFunction.Chunk.WriteByte((byte)addr, line);
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
    [MemberNotNull(nameof(_currentFunction))]
    [MemberNotNull(nameof(_localManager))]
    private void RequireFunction () {
        if (_currentFunction == null || _localManager == null) {
            throw new Exception(
                "This node can only be compiled in the context of a function."
            );
        }
    }
    #endregion
}
