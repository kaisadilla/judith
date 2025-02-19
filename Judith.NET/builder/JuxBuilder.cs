using Judith.NET.compiler.jub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.builder;

public class JuxBuilder {
    private string _outFolder;

    public JuxBuilder (string outFolder) {
        _outFolder = outFolder;
    }

    public void BuildBinary (string fileName, BinaryFile file) {
        string path = Path.Join(_outFolder, fileName);

        using var stream = File.Open(path, FileMode.Create);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, false);

        WriteMagicNumber(writer); // magic_number: byte[12] = 'AZARIAJUDITH'
        writer.Write((byte)0); // endianness: byte = 0 -- little-endian
        writer.Write((byte)0); // major_version: byte = 0
        writer.Write((byte)0); // minor_version: byte = 0

        // constant_count: ui32 -- the amount of constants in the table, not its size in bytes.
        writer.Write((uint)file.ConstantTable.Count);
        // constant_table: constant[constant_count]
        foreach (var ui8 in file.ConstantTable.Bytes) {
            writer.Write((byte)ui8);
        }

        // has_implicit: bool -- if true, the first function is the implicit function.
        writer.Write(file.HasImplicitFunction);

        // function_count: ui32 -- the amount of functions in the function table.
        writer.Write((uint)file.Functions.Count);
        // function_table: function[function_count]
        foreach (var func in file.Functions) {
            // name: ui32 -- the index of the function's name in the constant table.
            writer.Write((uint)func.NameIndex);
            // param_count: ui16 -- the arity of the function.
            writer.Write((ushort)func.Arity);
            
            foreach (var param in func.Parameters) {
                // name: ui32 -- the index of the parameter's name in the constant table.
                writer.Write((uint)param.NameIndex);
            }

            // max_locals: ui16 - The max amount of local slots the function needs.
            writer.Write((ushort)func.MaxLocals);
            // max_stack: ui16 - The max amount of stack slots the function needs.
            writer.Write((ushort)0);


            // code_length: ui32 -- the amount of bytes in the code bloc.
            writer.Write((uint)func.Chunk.Code.Count);
            // code: byte[code_length]
            foreach (var codeByte in func.Chunk.Code) {
                writer.Write((byte)codeByte);
            }

            // has_lines: bool -- if true, there's a bloc of i32 with the
            // same length as "code" mapping each code entry to a line in source.
            writer.Write(true);
            // lines: i32[code_length]
            foreach (var line in func.Chunk.Lines) {
                writer.Write((int)line);
            }
        }
    }

    private void WriteMagicNumber (BinaryWriter writer) {
        writer.Write((byte)'A');
        writer.Write((byte)'Z');
        writer.Write((byte)'A');
        writer.Write((byte)'R');
        writer.Write((byte)'I');
        writer.Write((byte)'A');
        writer.Write((byte)'J');
        writer.Write((byte)'U');
        writer.Write((byte)'D');
        writer.Write((byte)'I');
        writer.Write((byte)'T');
        writer.Write((byte)'H');
    }
}
