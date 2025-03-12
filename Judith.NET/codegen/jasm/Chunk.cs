using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.codegen.jasm;

public class Chunk {
    public List<byte> Code { get; private set; } = new();

    /// <summary>
    /// Returns the index where the next byte will be at. This is equal to
    /// Code.Count.
    /// </summary>
    public int NextIndex => Code.Count;
    /// <summary>
    /// Returns the index of the last byte in the code bloc. This is equal to
    /// Code.Count - 1.
    /// </summary>
    public int Index => Code.Count - 1;

    public void WriteInstruction (OpCode opCode) {
        Code.Add((byte)opCode);
    }

    public void WriteSByte (sbyte i8) {
        Code.Add(unchecked((byte)i8));
    }

    public void WriteByte (byte ui8) {
        Code.Add(ui8);
    }

    public void WriteUint16 (ushort u16) {
        WriteByte((byte)((u16 >> 0) & 0xff));
        WriteByte((byte)((u16 >> 8) & 0xff));
    }

    public void WriteInt32 (int i32) {
        WriteByte((byte)((i32 >> 0) & 0xff));
        WriteByte((byte)((i32 >> 8) & 0xff));
        WriteByte((byte)((i32 >> 16) & 0xff));
        WriteByte((byte)((i32 >> 24) & 0xff));
    }

    public void WriteUint32 (uint u32) {
        WriteByte((byte)((u32 >> 0) & 0xff));
        WriteByte((byte)((u32 >> 8) & 0xff));
        WriteByte((byte)((u32 >> 16) & 0xff));
        WriteByte((byte)((u32 >> 24) & 0xff));
    }

    public void WriteInt64 (long i64) {
        byte[] bytes = BitConverter.GetBytes(i64);
        if (BitConverter.IsLittleEndian == false) {
            Array.Reverse(bytes);
        }

        foreach (var b in bytes) {
            WriteByte(b);
        }
    }

    public void WriteFloat64 (double f64) {
        byte[] bytes = BitConverter.GetBytes(f64);
        if (BitConverter.IsLittleEndian == false) {
            Array.Reverse(bytes);
        }

        foreach (var b in bytes) {
            WriteByte(b);
        }
    }

    public void ExpandByte (int index, IEnumerable<byte> values) {
        Code.InsertRange(index, values);
    }

    /// <summary>
    /// Writes the STORE instruction required for the address given.
    /// </summary>
    /// <param name="addr">The address of the variable in the local variable array.</param>
    public void WriteStore (int addr) {
        if (addr == 0) {
            WriteInstruction(OpCode.STORE_0);
        }
        else if (addr == 1) {
            WriteInstruction(OpCode.STORE_1);
        }
        else if (addr == 2) {
            WriteInstruction(OpCode.STORE_2);
        }
        else if (addr == 3) {
            WriteInstruction(OpCode.STORE_3);
        }
        else if (addr == 4) {
            WriteInstruction(OpCode.STORE_4);
        }
        else if (addr <= byte.MaxValue) {
            WriteInstruction(OpCode.STORE);
            WriteByte((byte)addr);
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
    public void WriteLoad (int addr) {
        if (addr == 0) {
            WriteInstruction(OpCode.LOAD_0);
        }
        else if (addr == 1) {
            WriteInstruction(OpCode.LOAD_1);
        }
        else if (addr == 2) {
            WriteInstruction(OpCode.LOAD_2);
        }
        else if (addr == 3) {
            WriteInstruction(OpCode.LOAD_3);
        }
        else if (addr == 4) {
            WriteInstruction(OpCode.LOAD_4);
        }
        else if (addr <= byte.MaxValue) {
            WriteInstruction(OpCode.LOAD);
            WriteByte((byte)addr);
        }
        else {
            throw new NotImplementedException("VM does not yet support locals beyond 255");
            //Chunk.WriteInstruction(OpCode.LoadLong, line);
            //Chunk.WriteUint16((ushort)addr, line);
        }
    }

    public void WriteF64Const (double f64) {
        if (f64 == 0) {
            WriteInstruction(OpCode.CONST_0);
        }
        else if (f64 == 1) {
            WriteInstruction(OpCode.F_CONST_1);
        }
        else if (f64 == 2) {
            WriteInstruction(OpCode.F_CONST_2);
        }
        else {
            WriteInstruction(OpCode.CONST_LL);
            WriteFloat64(f64);
        }
    }

    public void WriteBoolConst (bool val) {
        if (val == true) {
            WriteInstruction(OpCode.I_CONST_1);
        }
        else {
            WriteInstruction(OpCode.CONST_0);
        }
    }

    public void WriteUtf8StringConst (int index) {
        if (index <= byte.MaxValue) {
            WriteInstruction(OpCode.STR_CONST);
            WriteByte((byte)index);
        }
        else {
            WriteInstruction(OpCode.STR_CONST_L);
            WriteInt32(index);
        }
    }

    public void WriteCall (int funcRefIndex) {
        WriteInstruction(OpCode.CALL);
        WriteUint32((uint)funcRefIndex);
    }


    /// <summary>
    /// Writes the short jump code given and a byte for the offset set at 0.
    /// Returns the index of the offset byte so it can be patched with PatchJump.
    /// </summary>
    /// <param name="code">The opcode to emit.</param>
    /// <param name="line">The line that caused this jump.</param>
    /// <returns></returns>
    public int WriteJump (OpCode code) {
        WriteInstruction(code);
        WriteSByte((sbyte)0);

        return Index;
    }

    public void WriteJumpBack (OpCode code, int targetIndex) {
        int offset = targetIndex - (Index + 2); // + 2 for the two bytes added by this jump.

        WriteInstruction(code);

        if (offset >= sbyte.MinValue || offset <= sbyte.MaxValue) {
            WriteSByte((sbyte)offset);
        }
        else {
            throw new NotImplementedException("Long jumps are not implemented");
        }
    }

    /// <summary>
    /// Patches the jump byte at the offset given so it points to the current
    /// instruction.
    /// </summary>
    /// <param name="indexByte">The byte that stores the jump offset.</param>
    public void PatchJump (int indexByte) {
        int offset = Index - indexByte;

        if (offset >= sbyte.MinValue || offset <= sbyte.MaxValue) {
            Code[indexByte] = (byte)((sbyte)offset);
        }
        else {
            throw new NotImplementedException("Long jumps are not implemented");
        }
    }

    public void PatchJumps (IEnumerable<int> offsets) {
        foreach (var o in offsets) PatchJump(o);
    }
}
