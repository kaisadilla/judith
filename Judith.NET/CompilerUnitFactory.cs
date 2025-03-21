using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET;

public static class CompilerUnitFactory {
    public static CompilerUnit FromNodeCollection (string fileName, ICollection<SyntaxNode> nodes) {
        List<Item> topLevelItems = [];
        List<SyntaxNode> implicitFunctionNodes = [];

        foreach (var node in nodes) {
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

        BlockBody body = new(implicitFunctionNodes);

        FunctionDefinition implicitFunc = new(
            true,
            true,
            new SimpleIdentifier(CompilerUnit.IMPLICIT_FUNCTION_NAME, true),
            parameters,
            null,
            body
        );

        return new(fileName, topLevelItems, implicitFunc);
    }

    private static T CastOrThrow<T> (SyntaxNode node) where T : SyntaxNode {
        if (node is T tNode) return tNode;

        throw new($"Syntax node of kind '{node.Kind}' doesn't have expected type.");
    }
}
