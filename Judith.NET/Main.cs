﻿// See https://aka.ms/new-console-template for more information
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

Lexer lexer = new(src);
lexer.Tokenize();
messages.Add(lexer.Messages);
// PrintTokens();
AbortIfError();

Parser parser = new(lexer.Tokens);
parser.Parse();
messages.Add(parser.Messages);
// PrintAST();
AbortIfError();

CompilerUnitBuilder cub = new(parser.Nodes);
cub.BuildUnit();
CompilerUnit cu = cub.CompilerUnit;

Compilation cmp = new([cu]);
cmp.Analyze();
messages.Add(cmp.Messages);

GenerateDebugFiles();
AbortIfError();

JubCompiler compiler = new(cmp);
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

void GenerateDebugFiles () {
    string symbolTableJson = JsonConvert.SerializeObject(cmp.SymbolTable, Formatting.Indented);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.symbol-table.json", symbolTableJson);

    string typeTableJson = JsonConvert.SerializeObject(cmp.TypeTable, Formatting.Indented);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.type-table.json", typeTableJson);

    string binderJson = JsonConvert.SerializeObject(cmp.Binder, Formatting.Indented);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.binder.json", binderJson);

    string astJson = JsonConvert.SerializeObject(cu, Formatting.Indented);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.ast.json", astJson);

    string msgJson = JsonConvert.SerializeObject(messages, Formatting.Indented);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.compile-messages.json", msgJson);

    var simpleAst = new SimpleAstPrinter().Visit(cu);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.simple-ast.txt", string.Join('\n', simpleAst));

    var semanticAst = new AstWithSemanticsPrinter(cmp).Visit(cu);
    string semanticAstStr = JsonConvert.SerializeObject(semanticAst, Formatting.Indented);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.semantic-ast.json", semanticAstStr);
}