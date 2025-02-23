using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET;

public class CompilerUnitBuilder {
    private string _fileName;
    private List<SyntaxNode> _nodes;

    public CompilerUnit? CompilerUnit { get; private set; } = null;

    public CompilerUnitBuilder (string fileName, List<SyntaxNode> nodes) {
        _fileName = fileName;
        _nodes = nodes;
    }

    [MemberNotNull(nameof(CompilerUnit))]
    public void BuildUnit () {
        List<Item> topLevelItems = new();
        List<SyntaxNode> implicitFunctionNodes = new();

        foreach (var node in _nodes) {
            switch (node.Kind) {
                case SyntaxKind.FunctionDefinition:
                case SyntaxKind.UserTypeDefinition:
                case SyntaxKind.AliasTypeDefinition:
                case SyntaxKind.UnionTypeDefinition:
                case SyntaxKind.SetTypeDefinition:
                case SyntaxKind.StructTypeDefinition:
                case SyntaxKind.InterfaceTypeDefinition:
                case SyntaxKind.ClassTypeDefinition:
                    topLevelItems.Add(CastOrThrow<Item>(node));
                    break;
                default:
                    implicitFunctionNodes.Add(node);
                    break;
            }
        }
        ParameterList parameters = new([
            //new Parameter(
            //    new LocalDeclarator(
            //        new Identifier("args", false),
            //        LocalKind.Constant,
            //        new(new("String", false)) // TODO: List<String>
            //    ),
            //    null
            //),
        ]);

        BlockStatement body = new(implicitFunctionNodes);

        FunctionDefinition implicitFunc = new(
            true,
            true,
            new Identifier(CompilerUnit.IMPLICIT_FUNCTION_NAME, true),
            parameters,
            null,
            body
        );

        CompilerUnit = new(_fileName, topLevelItems, implicitFunc);
    }

    private T CastOrThrow<T> (SyntaxNode node) where T : SyntaxNode {
        if (node is T tNode) return tNode;

        throw new($"Syntax node of kind '{node.Kind}' doesn't have expected type.");
    }
}
