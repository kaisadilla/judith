using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jub;

public enum ConstantType : byte {
    Error = 0,
    Int64,
    Float64,
    UnsignedInt64,
    StringASCII,
}

public class ConstantTable {
    public List<byte> Bytes { get; private init; } = new();
    /// <summary>
    /// A list where each index represents the offset of a constant at the
    /// given index in the byte array. This offset is at the position of the
    /// type byte: e.g. the first i64 is in the offset 0,
    /// while the second i64 is in the offset 9.
    /// </summary>
    public List<int> Offsets { get; private init; } = new();

    /// <summary>
    /// The size, in bytes, of the table.
    /// </summary>
    public int Size => Bytes.Count;
    /// <summary>
    /// The amount of constants in the table.
    /// </summary>
    public int Count => Offsets.Count;

    /// <summary>
    /// Adds an Int64 to the chunk and returns its index in the table.
    /// </summary>
    public int WriteInt64 (long i64) {
        Offsets.Add(Bytes.Count);
        Bytes.Add((byte)ConstantType.Int64);

        byte[] bytes = BitConverter.GetBytes(i64);
        if (BitConverter.IsLittleEndian == false) {
            Array.Reverse(bytes);
        }

        Bytes.AddRange(bytes);

        return Offsets.Count - 1;
    }

    /// <summary>
    /// Adds a Float64 to the chunk and returns its index in the table.
    /// </summary>
    public int WriteFloat64 (double f64) {
        Offsets.Add(Bytes.Count);
        Bytes.Add((byte)ConstantType.Float64);

        byte[] bytes = BitConverter.GetBytes(f64);
        if (BitConverter.IsLittleEndian == false) {
            Array.Reverse(bytes);
        }

        Bytes.AddRange(bytes);

        return Offsets.Count - 1;
    }

    /// <summary>
    /// Adds a UnsignedInt64 to the chunk and returns its index in the table.
    /// </summary>
    public int WriteUnsignedInt64 (ulong ui64) {
        Offsets.Add(Bytes.Count);
        Bytes.Add((byte)ConstantType.UnsignedInt64);

        byte[] bytes = BitConverter.GetBytes(ui64);
        if (BitConverter.IsLittleEndian == false) {
            Array.Reverse(bytes);
        }

        Bytes.AddRange(bytes);

        return Offsets.Count - 1;
    }

    /// <summary>
    /// Adds a String to the chunk and returns its index in the table.
    /// </summary>
    public int WriteStringASCII (string str) {
        Offsets.Add(Bytes.Count);
        Bytes.Add((byte)ConstantType.StringASCII);

        byte[] bytes = Encoding.ASCII.GetBytes(str);
        WriteUnsignedInt64((ulong)bytes.Length + 1); // +1 for null-terminator.

        Bytes.AddRange(bytes);
        Bytes.Add((byte)'\0'); // Null-terminator.

        return Offsets.Count - 1;
    }

    public long ReadInt64 (int offset) {
        return BitConverter.ToInt64(Bytes.ToArray(), offset);
    }

    public double ReadFloat64 (int offset) {
        return BitConverter.ToDouble(Bytes.ToArray(), offset);
    }

    public ulong ReadUnsignedInt64 (int offset) {
        return BitConverter.ToUInt64(Bytes.ToArray(), offset);
    }

    public string ReadStringASCII (int offset) {
        ulong size = ReadUnsignedInt64(offset);
        return Encoding.ASCII.GetString(
            Bytes.GetRange(offset + 1, (int)size).ToArray()
        );
    }
}
