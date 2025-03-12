using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.codegen.jasm;

public class StringTable {
    public List<byte> Bytes { get; private init; } = [];

    /// <summary>
    /// A list that links the index of each string to its offset.
    /// </summary>
    public List<int> Offsets { get; private init; } = [];

    /// <summary>
    /// The size, in bytes, of the table.
    /// </summary>
    public int Size => Bytes.Count;

    /// <summary>
    /// The amount of strings in the table.
    /// </summary>
    public int Count => Offsets.Count;

    private Dictionary<string, int> _existingStrings = [];

    public string this[int index] {
        get {
            int offset = Offsets[index];
            ulong size = ReadUnsignedInt64(offset);

            return Encoding.UTF8.GetString(
                CollectionsMarshal.AsSpan(Bytes).Slice(offset + 8, (int)size)
            );
        }
    }

    /// <summary>
    /// Returns the index of the string given in this table. If the string is
    /// not yet in this table, it gets added.
    /// </summary>
    /// <param name="str">The string whose index to get.</param>
    /// <returns></returns>
    public int GetStringIndex (string str) {
        if (_existingStrings.TryGetValue(str, out int index)) {
            return index;
        }

        Offsets.Add(Bytes.Count);

        byte[] bytes = Encoding.UTF8.GetBytes(str);
        PutUnsignedInt64((ulong)bytes.Length);

        Bytes.AddRange(bytes);
        _existingStrings[str] = Offsets.Count - 1;
        return Offsets.Count - 1;
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

    private ulong ReadUnsignedInt64 (int offset) {
        ReadOnlySpan<byte> ui64bytes = CollectionsMarshal.AsSpan(Bytes).Slice(offset, 8);
        return BitConverter.ToUInt64(ui64bytes);
    }
}
