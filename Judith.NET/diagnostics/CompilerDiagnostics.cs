using Judith.NET.analysis;
using Judith.NET.analysis.syntax;
using Judith.NET.compilation;
using Judith.NET.message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.diagnostics;

public static class CompilerDiagnostics {
    public static void GenerateCompilationFiles (
        IJudithCompiler compiler, string folderPath, string fileName
    ) {
        EmitMessages(compiler.Messages, folderPath, fileName);

        if (compiler.Tokens == null) return;
        EmitTokenList(compiler.Tokens, folderPath, fileName);

        if (compiler.Ast == null) return;
        EmitAst(compiler.Ast, folderPath, fileName);

        if (compiler.Compilation == null) return;

        foreach (var cu in compiler.Compilation.Program.Units) {
            EmitSimpleAst(cu, folderPath, fileName);
        }

        EmitSymbolTable(compiler.Compilation, folderPath, fileName);
        EmitBinder(compiler.Compilation, folderPath, fileName);

        if (compiler.Compilation.IsValidProgram == false) return;

        EmitTypeTable(compiler.Compilation, folderPath, fileName);
        EmitSemanticAst(compiler.Compilation, folderPath, fileName);
        EmitNodeTypes(compiler.Compilation, folderPath, fileName);
    }

    public static void EmitMessages (
        MessageContainer messages, string folderPath, string fileName
    ) {
        string json = Serialize(messages);
        WriteFile(folderPath, fileName + ".messages.json", json);
    }

    public static void EmitTokenList (
        List<Token> tokens, string folderPath, string fileName
    ) {
        string json = Serialize(tokens);
        WriteFile(folderPath, fileName + ".tokens.json", json);
    }

    public static void EmitAst (
        List<SyntaxNode> ast, string folderPath, string fileName
    ) {
        string json = Serialize(ast);
        WriteFile(folderPath, fileName + ".ast.json", json);
    }

    public static void EmitSimpleAst (
        CompilerUnit cu, string folderPath, string fileName
    ) {
        var simpleAst = string.Join('\n', new SimpleAstPrinter().Visit(cu));
        WriteFile(folderPath, fileName + ".simple-ast.txt", simpleAst);
    }

    public static void EmitSymbolTable (
        JudithCompilation cmp, string folderPath, string fileName
    ) {
        string json = Serialize(cmp.SymbolTable);
        WriteFile(folderPath, fileName + ".symbol-table.json", json);
    }

    public static void EmitBinder (
        JudithCompilation cmp, string folderPath, string fileName
    ) {
        string json = Serialize(cmp.Binder);
        WriteFile(folderPath, fileName + ".binder.json", json);
    }

    public static void EmitTypeTable (
        JudithCompilation cmp, string folderPath, string fileName
    ) {
        var gen = new TypeTableGenerator();
        gen.Analyze(cmp.SymbolTable);
        string json = Serialize(gen.TypeTable);
        WriteFile(folderPath, fileName + ".type-table.json", json);
    }

    public static void EmitSemanticAst (
        JudithCompilation cmp, string folderPath, string fileName
    ) {
        var gen = new AstWithSemanticsPrinter(cmp);
        string json = Serialize(gen.Visit(cmp.Program.Units[0]));
        WriteFile(folderPath, fileName + ".ast-semantic.json", json);
    }

    public static void EmitNodeTypes (
        JudithCompilation cmp, string folderPath, string fileName
    ) {
        var gen = new AstTypePrinter(cmp);
        gen.Analyze();
        string txt = string.Join('\n', gen.TypedNodes);
        WriteFile(folderPath, fileName + ".node-types.txt", txt);
    }

    private static string Serialize (object o) {
        return JsonConvert.SerializeObject(o, new JsonSerializerSettings() {
            Formatting = Formatting.Indented,
        });
    }

    private static void WriteFile (string folderPath, string filePath, string content) {
        File.WriteAllText(Path.Join(folderPath, filePath), content);
    }
}
