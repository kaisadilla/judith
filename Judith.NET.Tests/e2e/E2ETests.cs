using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        var output = Juvm("basic", "hello_world");
        Stdout.WriteLine("Output: " + output);
        Assert.True(output == "Hello world!\n");
    }

    #region Helper functions
    private string Jud (string path) {
        return Path.Join(_res, "/test_programs/", path + ".jud");
    }

    private string JuvmOut (string path) {
        return Path.Join(_res, "out", path + ".txt");
    }

    private string Juvm (string folderPath, string fileName) {
        string binPath = Compiler.Compile(folderPath, fileName);
        string outPath = JuvmOut(Path.Join(folderPath, fileName));

        var proc = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = _juvmPath,
                Arguments = Jud(binPath) + " " + outPath,
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

        return File.ReadAllText(outPath).Replace("\r\n", "\n");
    }
    #endregion
}
