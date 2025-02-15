// See https://aka.ms/new-console-template for more information
using Judith.NET;
using Judith.NET.analysis;
using Judith.NET.builder;
using Judith.NET.compiler;
using Judith.NET.compiler.jub;
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

JubCompiler compiler = new(parser.Nodes);
compiler.Compile();

Console.WriteLine("=== BIN DISASSEMBLY ===");
Console.WriteLine();
Console.WriteLine($"Functions ({compiler.Bin.Functions.Count}):");
for (int i = 0; i < compiler.Bin.Functions.Count; i++) {
    Console.WriteLine($"=== # {i:X4}");

    JasmDisassembler disassembler = new(compiler.Bin, compiler.Bin.Functions[i].Chunk);
    disassembler.Disassemble();

    Console.WriteLine(disassembler.Dump);
    Console.WriteLine("");
}

JuxBuilder builder = new(AppContext.BaseDirectory + "/res/");
builder.BuildBinary("test.jbin", compiler.Bin);

void PrintErrors () {
    Console.WriteLine($"Errors: {messages.Errors.Count} ---");
    foreach (var error in messages.Errors) {
        Console.WriteLine(error);
    }
}
