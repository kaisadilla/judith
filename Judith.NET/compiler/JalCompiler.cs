using Judith.NET.compiler.jal;
using Judith.NET.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler;

public class JalCompiler : SyntaxVisitor {
    private List<SyntaxNode> _ast;

    public JalChunk Chunk { get; private init; } = new();

    public JalCompiler (List<SyntaxNode> ast) {
        _ast = ast;
    }

    public void Compile () {
        Visit(_ast);
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
            Chunk.WriteInstruction(OpCode.Constant, node.Line);
            Chunk.WriteByte((byte)addr, node.Line);
        }
        else {
            Chunk.WriteInstruction(OpCode.ConstantLong, node.Line);
            Chunk.WriteInt32(addr, node.Line);
        }
    }

    public override void Visit (BinaryExpression node) {
        node.Left.Accept(this);
        node.Right.Accept(this);

        switch (node.Operator.OperatorKind) {
            case OperatorKind.Add:
                _Instr(OpCode.Add);
                return;
            case OperatorKind.Subtract:
                _Instr(OpCode.Subtract);
                return;
            case OperatorKind.Multiply:
                _Instr(OpCode.Multiply);
                return;
            case OperatorKind.Divide:
                _Instr(OpCode.Divide);
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
}
