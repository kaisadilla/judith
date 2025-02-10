using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jal;

public class JalChunk {
    public List<byte> Code { get; private set; } = new();
    public List<int> CodeLines { get; private set; } = new();
    public List<JalValue> Constants { get; private set; } = new();

    public void WriteByte (byte i8, int line) {
        Code.Add(i8);
        CodeLines.Add(line);
    }

    public void WriteInt32 (int i32, int line) {
        WriteByte((byte)((i32 >> 0) & 0xff), line);
        WriteByte((byte)((i32 >> 8) & 0xff), line);
        WriteByte((byte)((i32 >> 16) & 0xff), line);
        WriteByte((byte)((i32 >> 24) & 0xff), line);
    }

    public void WriteInstruction (OpCode opCode, int line) {
        Code.Add((byte)opCode);
        CodeLines.Add(line);
    }

    /// <summary>
    /// Adds a constant to the constant block in this chunk and returns its
    /// address.
    /// </summary>
    /// <param name="constant">The constant to add.</param>
    public int WriteConstant (JalValue constant) {
        Constants.Add(constant);
        return Constants.Count - 1;
    }
}
