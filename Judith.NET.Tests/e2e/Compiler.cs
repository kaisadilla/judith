using Judith.NET.analysis;
using Judith.NET.analysis.syntax;
using Judith.NET.builder;
using Judith.NET.compiler;
using Judith.NET.compiler.jub;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.Tests.e2e;

public static class Compiler {
    public static readonly string RES_PATH = Path.Join(
        AppContext.BaseDirectory, "res"
    );

    public static string Compile (string folderPath, string fileName) {
        string src = File.ReadAllText(
            Path.Join(RES_PATH, "jud", folderPath, fileName + ".jud")
        );

        MessageContainer messages = new();

        Lexer lexer = new(src);
        lexer.Tokenize();
        messages.Add(lexer.Messages);

        Parser parser = new(lexer.Tokens);
        parser.Parse();
        messages.Add(parser.Messages);

        var cu = CompilerUnitFactory.FromNodeCollection(fileName + ".jud", parser.Nodes);
        NativeCompilation nativeComp = NativeCompilation.Ver1();

        ProjectCompilation cmp = new([nativeComp], [cu]);
        cmp.Analyze();
        messages.Add(cmp.Messages);

        JubCompiler compiler = new(cmp);
        JudithDll dll = compiler.Compile();

        BinaryDllBuilder builder = new(Path.Join(RES_PATH, "bin", folderPath));
        builder.BuildLibrary(fileName + ".jdll", dll);

        return Path.Join(RES_PATH, "bin", folderPath, fileName + ".jdll");
    }
}
