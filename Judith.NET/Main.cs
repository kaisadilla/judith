// See https://aka.ms/new-console-template for more information
using Judith.NET;
using Judith.NET.analysis;
using Judith.NET.analysis.syntax;
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
// PrintTokens();
AbortIfError();

Parser parser = new(lexer.Tokens, messages);
parser.Parse();
// PrintAST();
AbortIfError();

string astJson = JsonConvert.SerializeObject(parser.Nodes, Formatting.Indented);
File.WriteAllText(AppContext.BaseDirectory + "/res/test.ast.json", astJson);

//SymbolCollectionAnalyzer symbolAnalyzer = new();
//symbolAnalyzer.Analyze(parser.Nodes);
//
//foreach (var function in symbolAnalyzer.ExistingFunctions) {
//    Console.WriteLine(function);
//}

CompilerUnitBuilder cub = new(parser.Nodes);
cub.BuildUnit();
CompilerUnit cu = cub.CompilerUnit;

Compilation comp = new([cu]);
comp.Analyze();

string symbolTableJson = JsonConvert.SerializeObject(comp.SymbolTable, Formatting.Indented);
File.WriteAllText(AppContext.BaseDirectory + "/res/test.symbol-table.json", symbolTableJson);

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



// Functions

void PrintErrors () {
    Console.WriteLine($"Errors: {messages.Errors.Count} ---");
    foreach (var error in messages.Errors) {
        Console.WriteLine(error);
    }
}

void AbortIfError () {
    if (messages.Errors.Count > 0) {
        Console.WriteLine("Errors found - Compilation aborted.");
        PrintErrors();
        return;
    }
}

void PrintTokens () {
    foreach (var token in lexer.Tokens!) {
        Console.WriteLine(token);
    }
    Console.WriteLine();

    PrintErrors();
}

void PrintAst () {
    Console.WriteLine();
    Console.WriteLine("=== AST ===");
    //Console.WriteLine(astJson);
    foreach (var node in parser.Nodes!) {
        Console.WriteLine(node);
    }

    Console.WriteLine();
    PrintErrors();
}