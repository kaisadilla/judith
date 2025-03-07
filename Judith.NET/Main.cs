using Judith.NET;
using Judith.NET.analysis;
using Judith.NET.analysis.syntax;
using Judith.NET.builder;
using Judith.NET.compiler;
using Judith.NET.compiler.jub;
using Judith.NET.diagnostics;
using Judith.NET.message;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("> juc test.judith");
Console.WriteLine();

string src = File.ReadAllText(AppContext.BaseDirectory + "/res/test.judith");

Stopwatch s = Stopwatch.StartNew();

MessageContainer messages = new();

// Generate token array:
Lexer lexer = new(src);
lexer.Tokenize();
messages.Add(lexer.Messages);
if (ShouldAbort()) return;

// Generate AST:
Parser parser = new(lexer.Tokens);
parser.Parse();
messages.Add(parser.Messages);
if (ShouldAbort()) return;

CompilerUnit cu = CompilerUnitFactory.FromNodeCollection("test", parser.Nodes);
NativeCompilation nativeComp = NativeCompilation.Ver1();

ProjectCompilation cmp = new([nativeComp], [cu]);
cmp.Analyze();
messages.Add(cmp.Messages);

GenerateDebugFiles();
if (ShouldAbort()) return;
JubCompiler compiler = new(cmp);
JudithDll dll = compiler.Compile();

Console.WriteLine("===============================");
Console.WriteLine("=====|| DLL DISASSEMBLY ||=====");
Console.WriteLine("===============================");
Console.WriteLine("");

DllDisassembler disassembler = new(dll);
disassembler.Disassemble();

BinaryDllBuilder builder = new(AppContext.BaseDirectory + "/res/");
builder.BuildLibrary($"test.jdll", dll);

s.Stop();
Console.WriteLine($"\nTotal build time: {s.ElapsedMilliseconds} ms");

#if RELEASE
Console.ReadKey(true);
#endif

#region Functions
void PrintErrors () {
    Console.WriteLine($"Errors: {messages.Errors.Count} ---");
    foreach (var error in messages.Errors) {
        Console.WriteLine(error);
    }
}

bool ShouldAbort () {
    if (messages.HasErrors) {
        Console.WriteLine("Errors found - Compilation aborted.");
        PrintErrors();
        return true;
    }
    return false;
}

void GenerateDebugFiles () {
    string symbolTableJson = JsonConvert.SerializeObject(cmp.SymbolTable, Formatting.Indented);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.symbol-table.json", symbolTableJson);

    string binderJson = JsonConvert.SerializeObject(cmp.Binder, Formatting.Indented);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.binder.json", binderJson);

    string tokensJson = JsonConvert.SerializeObject(lexer.Tokens, Formatting.Indented);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.tokens.json", tokensJson);

    string astJson = JsonConvert.SerializeObject(cu, Formatting.Indented);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.ast.json", astJson);

    string msgJson = JsonConvert.SerializeObject(messages, Formatting.Indented);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.compile-messages.json", msgJson);

    var simpleAst = new SimpleAstPrinter().Visit(cu);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.simple-ast.txt", string.Join('\n', simpleAst));

    if (messages.HasErrors) return;

    var typeTableGen = new TypeTableGenerator();
    typeTableGen.Analyze(cmp.SymbolTable);
    string typeTableJson = JsonConvert.SerializeObject(typeTableGen.TypeTable, Formatting.Indented);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.type-table.json", typeTableJson);

    var semanticAst = new AstWithSemanticsPrinter(cmp).Visit(cu);
    string semanticAstStr = JsonConvert.SerializeObject(semanticAst, Formatting.Indented);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.semantic-ast.json", semanticAstStr);

    var nodeTypes = new AstTypePrinter(cmp);
    nodeTypes.Analyze();
    string nodeTypeStr = string.Join("\n", nodeTypes.TypedNodes);
    File.WriteAllText(AppContext.BaseDirectory + "/res/test.node-types.txt", nodeTypeStr);
}
#endregion