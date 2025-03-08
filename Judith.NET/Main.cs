using Judith.NET;
using Judith.NET.analysis;
using Judith.NET.analysis.syntax;
using Judith.NET.builder;
using Judith.NET.codegen;
using Judith.NET.codegen.jasm;
using Judith.NET.compilation;
using Judith.NET.diagnostics;
using Judith.NET.message;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

string SRC_PATH = Path.Join(AppContext.BaseDirectory, "res", "test.jud");
string OUT_DIR = Path.Join(AppContext.BaseDirectory, "res", "out");

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine($"> juc - dev mode - target: '{SRC_PATH}'.\n");

string src = File.ReadAllText(SRC_PATH);

Stopwatch s = Stopwatch.StartNew();

var compiler = new JasmScriptCompiler("test", src);
compiler.Compile(Path.Join(OUT_DIR, "test.jdll"));

s.Stop();

Console.WriteLine(
    $"Total build time: {s.ElapsedMilliseconds} ms. " +
    $"Errors: {compiler.Messages.Errors.Count}."
);
Console.WriteLine("Generating debug files...");
CompilerDiagnostics.GenerateCompilationFiles(compiler, OUT_DIR, "test");
Console.WriteLine("Debug files generated.");

if (compiler.Assembly != null) {
    Console.WriteLine("");
    Console.WriteLine("===============================");
    Console.WriteLine("=====|| DLL DISASSEMBLY ||=====");
    Console.WriteLine("===============================");
    Console.WriteLine("");

    DllDisassembler disassembler = new(compiler.Assembly);
    disassembler.Disassemble();
}