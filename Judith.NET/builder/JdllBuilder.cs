using Judith.NET.codegen.jasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.builder;

public class JdllBuilder {
    private JasmAssembly _assembly;

    public JdllBuilder (JasmAssembly assembly) {
        _assembly = assembly;
    }

    public void BuildJdll (string outPath) {
        string dir = Path.GetDirectoryName(outPath) ?? throw new(
            $"Couldn't determine out path's directory. Out path: '{outPath}'"
        );

        using var mstream = new MemoryStream();
        using var writer = new BinaryWriter(mstream);

        WriteMagicNumber(writer); // magic_number: byte[6] = 'JUDITH'
        writer.Write((byte)0); // endianness: byte = 0 -- little-endian
        writer.Write((uint)_assembly.JudithVersion); // judith_version: byte = 0 - pre-alpha
        WriteVersion(writer, _assembly.Version); // version: Version

        WriteStringTable(writer, _assembly.NameTable); // name_count and name_table

        writer.Write((uint)0); // dep_count: Ui32
        // TODO: dependencies - this is ok because there's 0 dependencies.

        WriteRefTable(writer, _assembly.TypeRefTable); // type_ref_count and type_ref
        WriteRefTable(writer, _assembly.FunctionRefTable); // func_ref_count and func_ref

        writer.Write((uint)_assembly.Blocks.Count); // block_count

        foreach (var block in _assembly.Blocks) { // blocks: Block[block_count]
            WriteBlock(writer, block);
        }

        Directory.CreateDirectory(dir);
        File.WriteAllBytes(outPath, mstream.ToArray());
    }

    private void WriteMagicNumber (BinaryWriter writer) {
        writer.Write((byte)'J');
        writer.Write((byte)'U');
        writer.Write((byte)'D');
        writer.Write((byte)'I');
        writer.Write((byte)'T');
        writer.Write((byte)'H');
    }

    private void WriteVersion (BinaryWriter writer, Version version) {
        writer.Write((ushort)version.Major);
        writer.Write((ushort)version.Minor);
        writer.Write((ushort)version.Patch);
        writer.Write((ushort)version.Build);
    }

    public void WriteStringTable (BinaryWriter writer, StringTable table) {
        writer.Write((uint)table.Size); // table_size
        writer.Write((uint)table.Count); // name_count
        // name_table: string[string_count]
        foreach (var ui8 in table.Bytes) {
            writer.Write((byte)ui8);
        }
    }

    private void WriteRefTable (BinaryWriter writer, JasmRefTable table) {
        writer.Write((uint)table.Table.Count); // ref_count

        foreach (var itemRef in table.Table) {
            switch (itemRef) {
                case JasmInternalRef internalRef:
                    writer.Write((uint)internalRef.RefType); // ref_type (0)
                    writer.Write((uint)internalRef.Block); // block_index
                    writer.Write((uint)internalRef.Index); // item_index
                    break;
                case JasmNativeRef nativeRef:
                    writer.Write((uint)nativeRef.RefType); // ref_type (1)
                    writer.Write((uint)nativeRef.Index); // item_index (1)
                    break;
                case JasmExternalRef externalRef:
                    writer.Write((uint)externalRef.RefType); // ref_type (2)
                    writer.Write((uint)externalRef.BlockName); // block_name
                    writer.Write((uint)externalRef.ItemName); // item_name
                    break;
            }
        }
    }

    private void WriteBlock (BinaryWriter writer, JasmBlock block) {
        writer.Write((uint)block.NameIndex); // block_name

        WriteStringTable(writer, block.StringTable); // string_count and string_table

        writer.Write((uint)0); // type_count -- TODO
        // type_table -- TODO
        
        // func_count: ui32 -- the amount of functions in the function table.
        writer.Write((uint)block.FunctionTable.Count);

        // function_table: function[function_count]
        foreach (var func in block.FunctionTable) {
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
            writer.Write((ushort)0); // TODO


            // code_length: ui32 -- the amount of bytes in the code bloc.
            writer.Write((uint)func.Chunk.Code.Count);
            // code: byte[code_length]
            foreach (var codeByte in func.Chunk.Code) {
                writer.Write((byte)codeByte);
            }
        }
    }
}
