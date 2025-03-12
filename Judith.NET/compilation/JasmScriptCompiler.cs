﻿using Judith.NET.analysis;
using Judith.NET.analysis.syntax;
using Judith.NET.builder;
using Judith.NET.codegen;
using Judith.NET.codegen.jasm;
using Judith.NET.ir;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compilation;

/// <summary>
/// This compiler compiles a single Judith script to JASM.
/// </summary>
public class JasmScriptCompiler : IJudithCompiler {
    public string FileName { get; private set; }
    public string Source { get; private set; }

    public MessageContainer Messages { get; private set; } = new();

    /// <summary>
    /// The tokens contained in the source code, in order. This list is null
    /// until the tokenize step is complete.
    /// </summary>
    public List<Token>? Tokens { get; private set; } = null;
    /// <summary>
    /// The abstract syntax tree (AST) generated from the source code. This list
    /// is null until the parse step is complete.
    /// </summary>
    public List<SyntaxNode>? Ast { get; private set; } = null;

    public List<CompilerUnit>? CompilerUnits { get; private set; } = null;

    public NativeHeader? Native { get; private set; } = null;
    public List<AssemblyHeader>? Dependencies { get; private set; } = null;

    public Compilation? Compilation { get; private set; } = null;

    public List<IRBlock>? IR { get; private set; } = null;

    public JasmAssembly? Assembly { get; private set; }

    public JasmScriptCompiler (string fileName, string source) {
        FileName = fileName;
        Source = source;
    }

    public void Compile (string outPath) {
        // 1. Build AST.
        BuildAst();

        // 2. Collect dependencies.
        CreateCompilation();

        // 3. Analyze.
        Analyze();

        if (Compilation.IsValidProgram == false) {
            return;
        }

        // 4. Generate IR
        GenerateIR();

        // 5. Generate code
        GenerateCode();

        // 6. Build project
        BuildProject(outPath);
    }

    [MemberNotNull(nameof(Tokens), nameof(Ast))]
    public void BuildAst () {
        // 1. Tokenize
        Tokenize();
        // 2. Parse
        Parse();
    }

    [MemberNotNull(nameof(Tokens))]
    public void Tokenize () {
        Lexer lexer = new(Source);
        lexer.Tokenize();
        Messages.Add(lexer.Messages);

        Tokens = lexer.Tokens;
    }

    [MemberNotNull(nameof(Tokens), nameof(Ast))]
    public void Parse () {
        if (Tokens == null) throw new InvalidStepException(
            "The script cannot be parsed until it's been tokenized."
        );

        Parser parser = new(Tokens);
        parser.Parse();
        Messages.Add(parser.Messages);

        Ast = parser.Nodes;
    }

    [MemberNotNull(nameof(Compilation))]
    public void CreateCompilation () {
        // 1. Generate compmiler units
        GenerateCompilerUnits();

        // 2. Generate the native assembly.
        AddNativeAssembly();

        // 3. Add headers from dependencies.
        AddDependencies();

        // 4. Create compilation object
        CreateCompilationObject();
    }

    [MemberNotNull(nameof(Ast), nameof(CompilerUnits))]
    public void GenerateCompilerUnits () {
        if (Ast == null) throw new InvalidStepException(
            "Compiler units cannot be generated before the file is fully parsed."
        );

        CompilerUnits = [];

        CompilerUnits.Add(CompilerUnitFactory.FromNodeCollection(FileName, Ast));
    }

    [MemberNotNull(nameof(Native))]
    public void AddNativeAssembly () {
        Native = NativeHeader.Ver1();
    }

    [MemberNotNull(nameof(Dependencies))]
    public void AddDependencies () {
        Dependencies = [];
        // TODO: Actually add dependencies.
    }

    [MemberNotNull(nameof(Native), nameof(Dependencies), nameof(CompilerUnits), nameof(Compilation))]
    public void CreateCompilationObject () {
        if (Native == null || Dependencies == null || CompilerUnits == null) {
            throw new InvalidStepException(
                "Dependencies and compiler units have to be built before creating " +
                "the compilation object."
            );
        }

        Compilation = new(FileName, Native, Dependencies, CompilerUnits);
    }

    [MemberNotNull(nameof(Compilation))]
    public void Analyze () {
        if (Compilation == null) throw new InvalidStepException(
            "The compilation object has to be built before the compilation can " +
            "be analyzed."
        );

        Compilation.Analyze();
        Messages.Add(Compilation.Messages);
    }

    [MemberNotNull(nameof(IR))]
    public void GenerateIR () {
        if (Compilation == null) throw new InvalidStepException(
            "The compilation object has to be built before the IR can be generated. "
        );
        if (Compilation.IsValidProgram == false) throw new InvalidStepException(
            "Cannot generate IR unless the program is a valid program."
        );

        IR = [];

        var gen = new JudithIRGenerator(Compilation, IRNativeHeader.Ver1());
        foreach (var cu in Compilation.Units) {
             IR.Add(gen.GenerateBlock(cu));
        }
    }

    [MemberNotNull(nameof(Compilation), nameof(Assembly))]
    public void GenerateCode () {
        // TODO: Generate from IR instead.
        if (Compilation == null) throw new InvalidStepException(
            "The compilation object has to be built before the compilation can " +
            "be analyzed."
        );
        if (Compilation.IsValidProgram == false) throw new InvalidStepException(
            "Only valid programs can be compiled. Either the compilation has not" +
            "been analyzed, or it's not a valid program."
        );

        JasmGenerator jasmGen = new(Compilation);
        Assembly = jasmGen.Generate();
    }

    [MemberNotNull(nameof(Assembly))]
    public void BuildProject (string outPath) {
        if (Assembly == null) throw new InvalidStepException(
            "The assembly has to be generated to build it."
        );

        JdllBuilder builder = new(Assembly);
        builder.BuildJdll(outPath);
    }
}
