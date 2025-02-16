using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET;

public class CompilerUnitBuilder {
    private List<SyntaxNode> _nodes;

    public CompilerUnit? CompilerUnit { get; private set; } = null;

    public CompilerUnitBuilder (List<SyntaxNode> nodes) {
        _nodes = nodes;
    }

    [MemberNotNull(nameof(CompilerUnit))]
    public void BuildUnit () {
        List<Item> topLevelItems = new();
        List<SyntaxNode> implicitFunctionNodes = new();

        foreach (var node in _nodes) {
            implicitFunctionNodes.Add(node);
        }
        ParameterList parameters = new ParameterList([
            new Parameter(
                new LocalDeclarator(
                    new Identifier("args", false),
                    LocalKind.Constant,
                    null // TODO: List<String>
                ),
                null
            ),
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

        CompilerUnit = new(topLevelItems, implicitFunc);
    }
}
