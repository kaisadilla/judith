using Judith.NET.analysis;
using Judith.NET.analysis.binder;
using Judith.NET.analysis.semantics;
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

    private ProjectCompilation _cmp;
    private ScopeResolver _scope;
    private LocalBlock? _localBlock = null;

    /// <summary>
    /// A stack containing all the functions we are currently inside of. When
    /// we enter a new function, we place it at the top of the stack. When we
    /// exit it, that function is popped and the previous one becomes the current
    /// function once again.
    /// </summary>
    private Stack<BinaryFunction> _functions = new();

    public List<BinaryBlock> Blocks { get; private init; } = new();
    /// <summary>
    /// Contains all the function references available in this program.
    /// </summary>
    public FunctionRefTable FunctionRefs { get; private init; } = new();

    /// <summary>
    /// The current function, if we are inside one.
    /// </summary>
    private BinaryFunction? CurrentFunc => _functions.Count == 0
        ? null
        : _functions.Peek();

    private BinaryBlock CurrentBlock => Blocks[^1];
    private int CurrentBlockIndex => Blocks.Count - 1;

    public JubCompiler (ProjectCompilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp);
    }

    public JudithDll Compile () {
        CollectReferences();

        foreach (var unit in _cmp.Units) {
            Visit(unit);
        }

        return new(FunctionRefs, Blocks);
    }

    private void CollectReferences () {
        for (int i = 0; i < _cmp.Units.Count; i++) {
            CollectReferences(_cmp.Units[i], i);
        }
    }

    private void CollectReferences (CompilerUnit unit, int blockIndex) {
        int functionCount = 0;

        if (unit.ImplicitFunction != null) {
            _AddFunction(unit.ImplicitFunction);
        }

        foreach (var item in unit.TopLevelItems) {
            switch (item) {
                case FunctionDefinition funcDef:
                    _AddFunction(funcDef);
                    break;
            }
        }


        void _AddFunction (FunctionDefinition func) {
            var boundFunc = _cmp.Binder.GetBoundNodeOrThrow<BoundFunctionDefinition>(func);
            FunctionRefs.Add(boundFunc.Symbol.FullyQualifiedName, new(
                block: blockIndex,
                index: functionCount
            ));
            functionCount++;
        }
    }

    private void BeginFunction (string name) {
        if (CurrentFunc != null) {
            throw new NotImplementedException("Inner functions are not implemented!");
        }
        
        _localBlock = new(MAX_LOCALS);
        _functions.Push(new(CurrentBlock, name));
    }

    private void EndFunction () {
        RequireFunction();
        
        CurrentFunc.MaxLocals = _localBlock.MaxLocals;
        CurrentBlock.Functions.Add(CurrentFunc);

        _functions.Pop();
        _localBlock = null;
    }

    #region Compiling nodes
    public override void Visit (CompilerUnit node) {
        Blocks.Add(new(node.FileName));


        if (node.ImplicitFunction != null) {
            CurrentBlock.HasImplicitFunction = true;

            Visit(node.ImplicitFunction);
        }

        foreach (var item in node.TopLevelItems) {
            Visit(item);
        }
    }

    public override void Visit (FunctionDefinition node) {
        BeginFunction(node.Identifier.Name);
        _scope.BeginScope(node);
        Visit(node.Parameters);
        Visit(node.Body);
        _scope.EndScope();
        EndFunction();
    }

    public override void Visit (LocalDeclarationStatement node) {
        RequireFunction();

        if (node.DeclaratorList.Declarators.Count != 1) {
            throw new NotImplementedException("Multiple local declaration not yet implemented.");
        }

        string localName = node.DeclaratorList.Declarators[0].Identifier.Name;
        // Check if a local with this name is already declared.
        if (_localBlock.IsLocalDeclared(localName)) {
            throw new NotImplementedException("Local shadowing not yet implemented.");
        }

        int addr = _localBlock.AddLocal(localName);

        if (node.Initializer == null) return;

        Visit(node.Initializer);
        EmitStore(addr, node.Initializer.Line);
        _localBlock.MarkInitialized(addr);
    }

    public override void Visit (IfExpression node) {
        RequireFunction();

        Visit(node.Test);

        var thenJump = EmitJump(OpCode.JFALSE, node.Test.Line);

        _scope.BeginThenScope(node);
        _localBlock.BeginScope();
        Visit(node.Consequent);
        _localBlock.EndScope();
        _scope.EndScope();

        int elseJump = -1;
        if (node.Alternate != null) {
            elseJump = EmitJump(OpCode.JMP, node.Test.Line);
        }

        PatchJump(thenJump);

        if (node.Alternate != null) {
            _scope.BeginThenScope(node);
            _localBlock.BeginScope();
            Visit(node.Alternate);
            _localBlock.EndScope();
            _scope.EndScope();

            PatchJump(elseJump);
        }
    }

    public override void Visit (WhileExpression node) {
        RequireFunction();

        // Store where this loop ends.
        var loopStart = CurrentFunc.Chunk.Index;
        // Check condition
        Visit(node.Test);
        // Prepare a jump to skip the body if the test fails.
        var falseJump = EmitJump(OpCode.JFALSE, node.Test.Line);

        // Compile the body.
        _scope.BeginScope(node);
        _localBlock.BeginScope();
        Visit(node.Body);
        _localBlock.EndScope();
        _scope.EndScope();

        // Emit a jump back to the start of the loop.
        EmitJumpBack(OpCode.JMP, loopStart, node.Test.Line);
        // Point the skip body jump here.
        PatchJump(falseJump);
    }

    public override void Visit (ReturnStatement node) {
        RequireFunction();
        // TODO: Compile expression.

        CurrentFunc.Chunk.WriteInstruction(OpCode.RET, node.Line);
    }

    public override void Visit (AssignmentExpression node) {
        RequireFunction();

        if (node.Left.Kind == SyntaxKind.IdentifierExpression) {
            if (node.Left is not IdentifierExpression idExpr) {
                throw new Exception("Invalid type.");
            }

            if (_localBlock.TryGetLocalAddr(idExpr.Identifier.Name, out int addr) == false) {
                throw new Exception("Local not found");
            }

            EmitStore(addr, node.Line);
        }
        else if (node.Left.Kind == SyntaxKind.AccessExpression) {
            // TODO: Access expr.
        }
        else {
            throw new Exception("Can't assign to that."); // TODO: Analysis compile error.
        }

        Visit(node.Right);
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

    public override void Visit (CallExpression node) {
        Visit(node.Arguments);
        Visit(node.Callee);
    }

    public override void Visit (IdentifierExpression node) {
        RequireFunction();

        var boundNode = _cmp.Binder.GetBoundNodeOrThrow<BoundIdentifierExpression>(node);

        if (boundNode.Symbol.Kind == SymbolKind.Local || boundNode.Symbol.Kind == SymbolKind.Parameter) {
            if (_localBlock.TryGetLocalAddr(node.Identifier.Name, out int addr) == false) {
                throw new Exception($"Local '{node.Identifier.Name}' not found");
            }

            EmitLoad(addr, node.Line);
        }
        else if (boundNode.Symbol.Kind == SymbolKind.Function) {
            if (FunctionRefs.TryGetFunctionRef(
                boundNode.Symbol.FullyQualifiedName, out int funcRefIndex
            ) == false) throw new(
                $"Function reference for '{boundNode.Symbol.FullyQualifiedName}' not found."
            );

            CurrentFunc.Chunk.WriteInstruction(OpCode.CALL, node.Line);
            CurrentFunc.Chunk.WriteUint32((uint)funcRefIndex, node.Line);
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
            EmitF64Const(boundNode.Value.AsFloat, node.Line);
        }
        else if (boundNode.Type == _cmp.Native.Types.Int) {
            EmitI64Const(boundNode.Value.AsInteger, node.Line);
        }
        else if (boundNode.Type == _cmp.Native.Types.Bool) {
            if (boundNode.Value.AsBoolean == true) {
                CurrentFunc.Chunk.WriteInstruction(OpCode.I_CONST_1, node.Line);
            }
            else {
                CurrentFunc.Chunk.WriteInstruction(OpCode.CONST_0, node.Line);
            }
        }
        else if (boundNode.Type == _cmp.Native.Types.String) {
            int index = CurrentBlock.StringTable.WriteStringUtf8(boundNode.Value.AsString!);

            if (index <= byte.MaxValue) {
                CurrentFunc.Chunk.WriteInstruction(OpCode.STR_CONST, node.Line);
                CurrentFunc.Chunk.WriteByte((byte)index, node.Line);
            }
            else {
                CurrentFunc.Chunk.WriteInstruction(OpCode.STR_CONST_L, node.Line);
                CurrentFunc.Chunk.WriteInt32(index, node.Line);
            }
        }
        else {
            throw new NotImplementedException("Can't compile value of this type yet.");
        }
    }

    public override void Visit (EqualsValueClause node) {
        Visit(node.Value);
    }

    public override void Visit (ParameterList node) {
        Visit(node.Parameters);

        // We load parameter values from last to first, as the last argument
        // is at the top of the stack.
        for (int i = node.Parameters.Count - 1; i >= 0; i--) {
            EmitStore(i, node.Parameters[i].Line);
        }
    }

    public override void Visit (Parameter node) {
        RequireFunction();

        int addr = _localBlock.AddLocal(node.Declarator.Identifier.Name);
        _localBlock.MarkInitialized(addr);

        CurrentFunc.Parameters.Add(
            new(CurrentBlock, _cmp.Native.Types.Unresolved, node.Declarator.Identifier.Name)
        ); // TODO: Bind parameters and resolve their types. UnresolvedType is just a placeholder.
    }

    public override void Visit (P_PrintStatement node) {
        RequireFunction();

        Visit(node.Expression);

        if (_cmp.Binder.TryGetBoundNode(node.Expression, out BoundExpression? boundExpr) == false) {
            ThrowUnboundNode(node.Expression);
        }

        CurrentFunc.Chunk.WriteInstruction(OpCode.PRINT, node.Line);

        if (boundExpr.Type == _cmp.Native.Types.Num) {
            CurrentFunc.Chunk.WriteByte((byte)ConstantType.Float64, node.Expression.Line);
        }
        else if (boundExpr.Type == _cmp.Native.Types.String) {
            CurrentFunc.Chunk.WriteByte((byte)ConstantType.StringASCII, node.Expression.Line);
        }
        else {
            CurrentFunc.Chunk.WriteByte((byte)ConstantType.Bool, node.Expression.Line);
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
            jumpList.Add(EmitJump(OpCode.JFALSE_K, expr.Line));
        }
        else { // OperatorKind.LogicalOr
            jumpList.Add(EmitJump(OpCode.JTRUE_K, expr.Line));
        }

        Visit(expr.Right);
    }

    private void CompileRegularBinaryExpression (BinaryExpression node) {
        RequireFunction();

        Visit(node.Left);
        Visit(node.Right);

        switch (node.Operator.OperatorKind) {
            case OperatorKind.Add:
                _Instr(OpCode.F_ADD);
                return;
            case OperatorKind.Subtract:
                _Instr(OpCode.F_SUB);
                return;
            case OperatorKind.Multiply:
                _Instr(OpCode.F_MUL);
                return;
            case OperatorKind.Divide:
                _Instr(OpCode.F_DIV);
                return;
            case OperatorKind.Equals:
                _Instr(OpCode.EQ);
                return;
            case OperatorKind.NotEquals:
                _Instr(OpCode.NEQ);
                return;
            case OperatorKind.LessThan:
                _Instr(OpCode.F_LT);
                return;
            case OperatorKind.LessThanOrEqualTo:
                _Instr(OpCode.F_LE);
                return;
            case OperatorKind.GreaterThan:
                _Instr(OpCode.F_GT);
                return;
            case OperatorKind.GreaterThanOrEqualTo:
                _Instr(OpCode.F_GE);
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
            CurrentFunc.Chunk.WriteInstruction(opCode, node.Operator.Line);
        }
    }

    private T CastOrThrow<T> (SyntaxNode node) where T : SyntaxNode {
        if (node is not T tNode) {
            throw new Exception("Class and SyntaxKind mismatch.");
        }

        return tNode;
    }

    private void EmitF64Const (double f64, int line) {
        RequireFunction();

        if (f64 == 0) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.CONST_0, line);
        }
        else if (f64 == 1) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.F_CONST_1, line);
        }
        else if (f64 == 2) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.F_CONST_2, line);
        }
        else {
            CurrentFunc.Chunk.WriteInstruction(OpCode.CONST_LL, line);
            CurrentFunc.Chunk.WriteFloat64(f64, line);
        }
    }

    private void EmitI64Const (long i64, int line) {
        RequireFunction();

        if (i64 == 0) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.CONST_0, line);
        }
        else if (i64 == 1) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.I_CONST_1, line);
        }
        else if (i64 == 2) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.I_CONST_2, line);
        }
        else if (i64 >= sbyte.MinValue && i64 <= sbyte.MaxValue) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.CONST, line);
            CurrentFunc.Chunk.WriteSByte((sbyte)i64, line);
        }
        else if (i64 >= int.MinValue && i64 <= int.MaxValue) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.CONST_L, line);
            CurrentFunc.Chunk.WriteInt32((int)i64, line);
        }
        else {
            CurrentFunc.Chunk.WriteInstruction(OpCode.CONST_LL, line);
            CurrentFunc.Chunk.WriteInt64(i64, line);
        }
    }

    /// <summary>
    /// Writes the STORE instruction required for the address given.
    /// </summary>
    /// <param name="addr">The address of the variable in the local variable array.</param>
    /// <param name="line">The line of code that produced this.</param>
    private void EmitStore (int addr, int line) {
        RequireFunction();

        if (addr == 0) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.STORE_0, line);
        }
        else if (addr == 1) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.STORE_1, line);
        }
        else if (addr == 2) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.STORE_2, line);
        }
        else if (addr == 3) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.STORE_3, line);
        }
        else if (addr == 4) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.STORE_4, line);
        }
        else if (addr <= byte.MaxValue) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.STORE, line);
            CurrentFunc.Chunk.WriteByte((byte)addr, line);
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
            CurrentFunc.Chunk.WriteInstruction(OpCode.LOAD_0, line);
        }
        else if (addr == 1) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.LOAD_1, line);
        }
        else if (addr == 2) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.LOAD_2, line);
        }
        else if (addr == 3) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.LOAD_3, line);
        }
        else if (addr == 4) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.LOAD_4, line);
        }
        else if (addr <= byte.MaxValue) {
            CurrentFunc.Chunk.WriteInstruction(OpCode.LOAD, line);
            CurrentFunc.Chunk.WriteByte((byte)addr, line);
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

        CurrentFunc.Chunk.WriteInstruction(code, line);
        CurrentFunc.Chunk.WriteByte(0, line);

        return CurrentFunc.Chunk.Index;
    }

    /// <summary>
    /// Patches the jump byte at the offset given so it points to the current
    /// instruction.
    /// </summary>
    /// <param name="indexByte">The byte that stores the jump offset.</param>
    private void PatchJump (int indexByte) {
        RequireFunction();

        int offset = CurrentFunc.Chunk.Index - indexByte;

        if (offset >= sbyte.MinValue || offset <= sbyte.MaxValue) {
            CurrentFunc.Chunk.Code[indexByte] = (byte)((sbyte)offset);
        }
        else {
            throw new NotImplementedException("Long jumps are not implemented");
        }
    }

    private void EmitJumpBack (OpCode code, int targetIndex, int line) {
        RequireFunction();

        int offset = targetIndex - (CurrentFunc.Chunk.Index + 2); // + 2 for the two bytes added by this jump.

        CurrentFunc.Chunk.WriteInstruction(code, line);

        if (offset >= sbyte.MinValue || offset <= sbyte.MaxValue) {
            CurrentFunc.Chunk.WriteSByte((sbyte)offset, line);
        }
        else {
            throw new NotImplementedException("Long jumps are not implemented");
        }
    }

    private void PatchJumps (IEnumerable<int> offsets) {
        foreach (var o in offsets) PatchJump(o);
    }

    #region Requires
    /// <summary>
    /// Checks that the current context is that of a function, asserting that
    /// CurrentFunc and _localManager exist.
    /// </summary>
    /// <exception cref="Exception"></exception>
    [MemberNotNull(nameof(CurrentFunc))]
    [MemberNotNull(nameof(_localBlock))]
    private void RequireFunction () {
        if (CurrentFunc == null || _localBlock == null) {
            throw new Exception(
                "This node can only be compiled in the context of a function."
            );
        }
    }

    [DoesNotReturn]
    private void ThrowUnboundNode (SyntaxNode node) {
        throw new Exception($"Cannot compile incomplete bound node '{node}'");
    }
    #endregion Requires
    #endregion Compiling nodes
}
