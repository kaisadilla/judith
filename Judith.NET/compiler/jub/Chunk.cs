using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jub;

public class Chunk {
    public List<byte> Code { get; private set; } = new();
    public List<int> CodeLines { get; private set; } = new();

    public void WriteByte (byte i8, int line) {
        Code.Add(i8);
        CodeLines.Add(line);
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

    public void WriteInstruction (OpCode opCode, int line) {
        Code.Add((byte)opCode);
        CodeLines.Add(line);
    }
}
