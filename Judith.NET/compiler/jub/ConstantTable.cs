using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jub;

/// <summary>
/// The type of a constant from the constant table. This is unrelated to Judith
/// types, but instead refers to the different types of data the VM can handle.
/// </summary>
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

    private Dictionary<long, int> _existingInt64s = new();
    private Dictionary<double, int> _existingFloat64s = new();
    private Dictionary<ulong, int> _existingUnsignedInt64s = new();
    private Dictionary<string, int> _existingStrings = new();

    /// <summary>
    /// Searches the given Int64 in the table and returns its index. If the
    /// value is not yet on the table, it's added to it.
    /// </summary>
    public int WriteInt64 (long i64) {
        if (_existingInt64s.TryGetValue(i64, out int index)) {
            return index;
        }

        Offsets.Add(Bytes.Count);
        Bytes.Add((byte)ConstantType.Int64);

        byte[] bytes = BitConverter.GetBytes(i64);
        if (BitConverter.IsLittleEndian == false) {
            Array.Reverse(bytes);
        }

        Bytes.AddRange(bytes);

        _existingInt64s[i64] = Offsets.Count - 1;
        return Offsets.Count - 1;
    }

    /// <summary>
    /// Searches the given Float64 in the table and returns its index. If the
    /// value is not yet on the table, it's added to it.
    /// </summary>
    public int WriteFloat64 (double f64) {
        if (_existingFloat64s.TryGetValue(f64, out int index)) {
            return index;
        }

        Offsets.Add(Bytes.Count);
        Bytes.Add((byte)ConstantType.Float64);

        byte[] bytes = BitConverter.GetBytes(f64);
        if (BitConverter.IsLittleEndian == false) {
            Array.Reverse(bytes);
        }

        Bytes.AddRange(bytes);

        _existingFloat64s[f64] = Offsets.Count - 1;
        return Offsets.Count - 1;
    }

    /// <summary>
    /// Searches the given UnsignedInt64 in the table and returns its index. If
    /// the value is not yet on the table, it's added to it.
    /// </summary>
    public int WriteUnsignedInt64 (ulong ui64) {
        if (_existingUnsignedInt64s.TryGetValue(ui64, out int index)) {
            return index;
        }

        Offsets.Add(Bytes.Count);
        Bytes.Add((byte)ConstantType.UnsignedInt64);

        PutUnsignedInt64(ui64);

        _existingUnsignedInt64s[ui64] = Offsets.Count - 1;
        return Offsets.Count - 1;
    }

    /// Searches the given String in the table and returns its index. If the
    /// value is not yet on the table, it's added to it.
    public int WriteStringASCII (string str) {
        if (_existingStrings.TryGetValue(str, out int index)) {
            return index;
        }

        Offsets.Add(Bytes.Count);
        Bytes.Add((byte)ConstantType.StringASCII);

        byte[] bytes = Encoding.ASCII.GetBytes(str);
        PutUnsignedInt64((ulong)bytes.Length + 1); // +1 for null-terminator.

        Bytes.AddRange(bytes);
        Bytes.Add((byte)'\0'); // Null-terminator.

        _existingStrings[str] = Offsets.Count - 1;
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
            Bytes.GetRange(offset + sizeof(ulong), (int)size).ToArray()
        );
    }

    /// <summary>
    /// Puts the bytes for an UnsignedInt64 into the table (without creating
    /// a new value).
    /// </summary>
    /// <param name="ui64"></param>
    private void PutUnsignedInt64 (ulong ui64) {
        byte[] bytes = BitConverter.GetBytes(ui64);
        if (BitConverter.IsLittleEndian == false) {
            Array.Reverse(bytes);
        }

        Bytes.AddRange(bytes);
    }
}
