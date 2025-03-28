﻿using Judith.NET.compilation;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Judith.NET.Tests.e2e;

public class E2ETests {
    private ITestOutputHelper Stdout { get; }

    private string _juvmPath;
    private string _res;

    public E2ETests (ITestOutputHelper output) {
        Stdout = output;
        _juvmPath = File.ReadAllText(AppContext.BaseDirectory + "/res/juvm");
        _res = AppContext.BaseDirectory + "/res";
    }

    [Fact]
    public void JuvmFound () {
        Stdout.WriteLine("juvm path: " + _juvmPath);
        Assert.True(File.Exists(_juvmPath));
    }

    [Fact]
    public void HelloWorld () {
        var output = ScriptRun("basic", "hello_world");
        Stdout.WriteLine("Output: " + output);
        Assert.True(output == "Hello world!\n");
    }

    [Theory]
    [InlineData("basic", "while1")]
    [InlineData("basic", "comparisons1")]
    [InlineData("basic", "fibonacci1")]
    [InlineData("basic", "factorial1")]
    [InlineData("basic", "comment1")]
    public void ValidScripts (string folderPath, string fileName) {
        try {
            var output = ScriptRun(folderPath, fileName);
            var expected = ScriptExpected(folderPath, fileName);
            Stdout.WriteLine("Output: " + output);
            Assert.True(output == expected);
        }
        catch (CompilationException ex) {
            Console.WriteLine("Invalid program: ");
            foreach (var err in ex.Compiler.Messages.Errors) {
                Console.WriteLine(err.Message);
            }
            Assert.True(false);
        }
    }

    #region Helper functions
    private string ScriptRun (string folderPath, string fileName) {
        string srcPath = Path.Join(_res, "scripts", "programs", folderPath, fileName + ".jud");
        string binPath = Path.Join(_res, "scripts", "out", folderPath, fileName + ".jdll");
        string outPath = Path.Join(_res, "scripts", "out", folderPath, fileName + ".txt");

        string src = File.ReadAllText(srcPath);

        var compiler = new JasmScriptCompiler("test", src);
        compiler.Compile(binPath);

        if (compiler.Messages.HasErrors) {
            throw new CompilationException(compiler);
        }

        var proc = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = _juvmPath,
                Arguments = binPath + " " + outPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.WaitForExit();

        if (proc.ExitCode != 0) {
            throw new Exception("JuVM crashed.");
        }

        return File.ReadAllText(outPath).Replace("\r\n", "\n");
    }

    private string ScriptExpected (string folderPath, string fileName) {
        string path = Path.Join(
            _res, "scripts", "expected", folderPath, fileName + ".txt"
        );

        return File.ReadAllText(path).Replace("\r\n", "\n");
    }
    #endregion
}

file class CompilationException : Exception {
    public JasmScriptCompiler Compiler { get; private init; }

    public CompilationException (JasmScriptCompiler compiler) {
        Compiler = compiler;
    }
}
