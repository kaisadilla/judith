// See https://aka.ms/new-console-template for more information
using Judith.NET;
using Judith.NET.analysis;
using Judith.NET.compiler;
using Judith.NET.compiler.jal;
using Judith.NET.diagnostics;
using Judith.NET.message;
using Newtonsoft.Json;
using System.Text;

Console.WriteLine("> judith test.judith");
Console.WriteLine();

string src = File.ReadAllText(AppContext.BaseDirectory + "/res/test.judith");

MessageContainer messages = new();
Lexer lexer = new(src, messages);

lexer.Tokenize();

foreach (var token in lexer.Tokens!) {
    Console.WriteLine(token);
}
Console.WriteLine();

PrintErrors();

Parser parser = new(lexer.Tokens, messages);
parser.Parse();
string astJson = JsonConvert.SerializeObject(parser.Nodes, Formatting.Indented);

Console.WriteLine();
Console.WriteLine("=== AST ===");
//Console.WriteLine(astJson);
foreach (var node in parser.Nodes) {
    Console.WriteLine(node);
}

File.WriteAllText(AppContext.BaseDirectory + "/res/test.ast.json", astJson);
Console.WriteLine();
PrintErrors();

if (messages.Errors.Count > 0) {
    Console.WriteLine("Compilation aborted.");
    return;
}

SymbolCollectionAnalyzer symbolAnalyzer = new();
symbolAnalyzer.Analyze(parser.Nodes);

foreach (var function in symbolAnalyzer.ExistingFunctions) {
    Console.WriteLine(function);
}

//JalChunk chunk = new();
////chunk.WriteByte(OpCode.NoOp);
////chunk.WriteByte(OpCode.Return);
////chunk.CodeLines.Add(5);
////chunk.CodeLines.Add(6);
//int addr = chunk.WriteConstant(new JalValue<double>(JalValueType.Float64, 18d));
//chunk.WriteInstruction(OpCode.NoOp, 6);
//chunk.WriteInstruction(OpCode.Constant, 7);
//chunk.WriteByte((byte)addr, 7);
//chunk.WriteInstruction(OpCode.Return, 8);

JalCompiler compiler = new(parser.Nodes);
compiler.Compile();
JalChunk chunk = compiler.Chunk;

JalDisassembler disassembler = new(chunk);
disassembler.Disassemble();

Console.WriteLine();
Console.WriteLine("=== CHUNK DISASSEMBLY ===");
Console.WriteLine();
Console.WriteLine(disassembler.Dump);

SaveChunkFile(chunk);

void PrintErrors () {
    Console.WriteLine($"Errors: {messages.Errors.Count} ---");
    foreach (var error in messages.Errors) {
        Console.WriteLine(error);
    }
}

void SaveChunkFile (JalChunk chunk) {
    string path = AppContext.BaseDirectory + "/res/test.jbin";

    using var stream = File.Open(path, FileMode.Create);
    using var writer = new BinaryWriter(stream, Encoding.UTF8, false);

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

    // constantCount (i32)
    writer.Write((int)chunk.Constants.Count);
    // constants (f64)
    foreach (var constant in chunk.Constants) {
        if (constant is JalValue<double> c_dbl) {
            //writer.Write((byte)constant.Type);
            writer.Write((double)c_dbl.Value);
        }
    }

    // size (i32)
    writer.Write((int)chunk.Code.Count);
    // code (ui8[])
    foreach (var code in chunk.Code) {
        writer.Write((byte)code);
    }

    // containsLines
    writer.Write(true);
    // lines (i32[])
    foreach (var line in chunk.CodeLines) {
        writer.Write((int)line);
    }
}
