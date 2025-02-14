using Judith.NET.compiler.jal;
using Judith.NET.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Judith.NET.compiler;

record Local (string Name, int depth) {
    public readonly string Name = Name;
    public readonly int Depth = depth;
    public bool Initialized = false;
}

public class JalCompiler : SyntaxVisitor {
    const int MAX_LOCALS = ushort.MaxValue + 1;

    private List<SyntaxNode> _ast;

    private int _scopeDepth = 0;
    private List<Local> _locals = new();

    public JalChunk Chunk { get; private init; } = new();

    public JalCompiler (List<SyntaxNode> ast) {
        _ast = ast;
    }

    public void Compile () {
        Visit(_ast);
    }
    
    private void BeginScope () {
        _scopeDepth++;
    }

    private void EndScope () {
        _scopeDepth--;
    }

    public override void Visit (LocalDeclarationStatement node) {
        if (node.DeclaratorList.Declarators.Count != 1) {
            throw new NotImplementedException("Multiple local declaration not yet implemented.");
        }

        if (_locals.Count >= MAX_LOCALS) {
            throw new Exception("Too many locals."); // TODO: Compile error.
        }

        // Check existing locals.
        foreach (var otherLocal in _locals) {
            if (otherLocal.Name == node.DeclaratorList.Declarators[0].Identifier.Name) {
                throw new NotImplementedException("Local shadowing not yet implemented.");
            }
        }

        Local local = new(node.DeclaratorList.Declarators[0].Identifier.Name, _scopeDepth);
        _locals.Add(local);
        int addr = _locals.Count - 1;

        if (node.Initializer == null) return;

        Visit(node.Initializer);
        WriteStore(addr, node.Initializer.Line);
        local.Initialized = true;
    }

    public override void Visit (EqualsValueClause node) {
        Visit(node.Value);
    }

    public override void Visit (LiteralExpression node) {
        int addr;
        if (node.Literal.LiteralKind == LiteralKind.Float64) {
            if (node.Literal.Value is FloatValue fval) {
                addr = Chunk.WriteConstant(
                    new JalValue<double>(JalValueType.Float64, fval.Value)
                );
            }
            else {
                throw new Exception("Literal node (F64) has invalid value.");
            }
        }
        else if (node.Literal.LiteralKind == LiteralKind.Int64) {
            if (node.Literal.Value is IntegerValue ival) {
                addr = Chunk.WriteConstant(
                    new JalValue<double>(JalValueType.Float64, ival.Value)
                );
            }
            else {
                throw new Exception("Literal node (I64) has invalid value.");
            }
        }
        else {
            throw new NotImplementedException("Literals of this type cannot yet be added to the constant stack.");
        }

        if (addr < byte.MaxValue + 1) {
            Chunk.WriteInstruction(OpCode.Const, node.Line);
            Chunk.WriteByte((byte)addr, node.Line);
        }
        else {
            Chunk.WriteInstruction(OpCode.ConstLong, node.Line);
            Chunk.WriteInt32(addr, node.Line);
        }
    }

    public override void Visit (IdentifierExpression node) {
        int? addr = null;

        for (int i = 0; i < _locals.Count; i++) {
            if (_locals[i].Name == node.Identifier.Name) {
                if (_locals[i].Initialized == false) {
                    throw new Exception("Local not initialized."); // TODO: Compile error.
                }
                addr = i;
                break;
            }
        }

        if (addr == null) {
            throw new Exception("Local not found."); // TODO: Compile error.
        }

        WriteLoad(addr.Value, node.Line);
    }

    public override void Visit (ReturnStatement node) {
        // TODO: Compile expression.

        Chunk.WriteInstruction(OpCode.Ret, node.Line);
    }

    public override void Visit (BinaryExpression node) {
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
            Chunk.WriteInstruction(opCode, node.Operator.Line);
        }
    }

    public override void Visit (AssignmentExpression node) {
        if (node.Left.Kind != SyntaxKind.IdentifierExpression) {
            throw new Exception("Can't assign to that."); // TODO: Compile error.
        }
        if (node.Left is not IdentifierExpression idExpr) {
            throw new Exception("Invalid type.");
        }

        Visit(node.Right);

        int? addr = null;

        for (int i = 0; i < _locals.Count; i++) {
            if (_locals[i].Name == idExpr.Identifier.Name) {
                addr = i;
                break;
            }
        }

        if (addr == null) {
            throw new Exception("Local not found."); // TODO: Compile error.
        }

        WriteStore(addr.Value, node.Line);
    }

    public override void Visit (PrivPrintStmt node) {
        Visit(node.Expression);

        Chunk.WriteInstruction(OpCode.Print, node.Line);
    }

    /// <summary>
    /// Writes the STORE instruction required for the address given.
    /// </summary>
    /// <param name="addr">The address of the variable in the local variable array.</param>
    /// <param name="line">The line of code that produced this.</param>
    private void WriteStore (int addr, int line) {
        if (addr == 0) {
            Chunk.WriteInstruction(OpCode.Store0, line);
        }
        else if (addr == 1) {
            Chunk.WriteInstruction(OpCode.Store1, line);
        }
        else if (addr == 2) {
            Chunk.WriteInstruction(OpCode.Store2, line);
        }
        else if (addr == 3) {
            Chunk.WriteInstruction(OpCode.Store3, line);
        }
        else if (addr == 4) {
            Chunk.WriteInstruction(OpCode.Store4, line);
        }
        else if (addr <= byte.MaxValue) {
            Chunk.WriteInstruction(OpCode.Store, line);
            Chunk.WriteByte((byte)addr, line);
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
        if (addr == 0) {
            Chunk.WriteInstruction(OpCode.Load0, line);
        }
        else if (addr == 1) {
            Chunk.WriteInstruction(OpCode.Load1, line);
        }
        else if (addr == 2) {
            Chunk.WriteInstruction(OpCode.Load2, line);
        }
        else if (addr == 3) {
            Chunk.WriteInstruction(OpCode.Load3, line);
        }
        else if (addr == 4) {
            Chunk.WriteInstruction(OpCode.Load4, line);
        }
        else if (addr <= byte.MaxValue) {
            Chunk.WriteInstruction(OpCode.Load, line);
            Chunk.WriteByte((byte)addr, line);
        }
        else {
            throw new NotImplementedException("VM does not yet support locals beyond 255");
            //Chunk.WriteInstruction(OpCode.LoadLong, line);
            //Chunk.WriteUint16((ushort)addr, line);
        }
    }
}
