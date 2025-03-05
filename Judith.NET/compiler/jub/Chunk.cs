using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jub;

public class Chunk {
    public List<byte> Code { get; private set; } = new();
    public List<int> Lines { get; private set; } = new();

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

    public void WriteSByte (sbyte i8, int line) {
        Code.Add(unchecked((byte)i8));
        Lines.Add(line);
    }

    public void WriteByte (byte ui8, int line) {
        Code.Add(ui8);
        Lines.Add(line);
    }

    public void WriteUint16 (ushort u16, int line) {
        WriteByte((byte)((u16 >> 0) & 0xff), line);
        WriteByte((byte)((u16 >> 8) & 0xff), line);
    }

    public void WriteInt32 (int i32, int line) {
        WriteByte((byte)((i32 >> 0) & 0xff), line);
        WriteByte((byte)((i32 >> 8) & 0xff), line);
        WriteByte((byte)((i32 >> 16) & 0xff), line);
        WriteByte((byte)((i32 >> 24) & 0xff), line);
    }

    public void WriteUint32 (uint u32, int line) {
        WriteByte((byte)((u32 >> 0) & 0xff), line);
        WriteByte((byte)((u32 >> 8) & 0xff), line);
        WriteByte((byte)((u32 >> 16) & 0xff), line);
        WriteByte((byte)((u32 >> 24) & 0xff), line);
    }

    public void WriteInt64 (long i64, int line) {
        byte[] bytes = BitConverter.GetBytes(i64);
        if (BitConverter.IsLittleEndian == false) {
            Array.Reverse(bytes);
        }

        foreach (var b in bytes) {
            WriteByte(b, line);
        }
    }

    public void WriteFloat64 (double f64, int line) {
        byte[] bytes = BitConverter.GetBytes(f64);
        if (BitConverter.IsLittleEndian == false) {
            Array.Reverse(bytes);
        }

        foreach (var b in bytes) {
            WriteByte(b, line);
        }
    }

    public void ExpandByte (int index, IEnumerable<byte> values, int line) {
        Code.InsertRange(index, values);

        IEnumerable<int> lines = values.Select(v => line);
        Lines.InsertRange(index, lines);
    }

    public void WriteInstruction (OpCode opCode, int line) {
        Code.Add((byte)opCode);
        Lines.Add(line);
    }
}
