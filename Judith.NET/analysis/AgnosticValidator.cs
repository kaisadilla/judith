using Judith.NET.analysis.syntax;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

/// <summary>
/// Validates that every use of a node is valid in its context, for rules that
/// don't need any prior semantic analysis.
/// </summary>
public class AgnosticValidator : SyntaxVisitor {
    public MessageContainer Messages { get; private set; } = new();

    private readonly JudithCompilation _cmp;

    public AgnosticValidator (JudithCompilation cmp) {
        _cmp = cmp;
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    public override void Visit (LocalDeclarationStatement node) {
        if (node.DeclaratorList.DeclaratorKind == LocalDeclaratorKind.Regular) {
            if (node.Initializer == null) {
                if (node.DeclaratorList.Declarators[^1].TypeAnnotation == null) {
                    Messages.Add(
                        CompilerMessage.Analyzers.UninitializedDeclaratorsMustHaveType(node)
                    );
                }
                return;
            }
            else {
                if (node.DeclaratorList.Declarators.Count != node.Initializer.Values.Count) {
                    Messages.Add(CompilerMessage.Analyzers.InitializersMustMatchVariables(node));
                    return;
                }
            }
        }
    }

    public override void Visit (ExpressionStatement node) {
        // Check that only relevant expressions are used in expression statements.
        switch (node.Expression.Kind) {
            case SyntaxKind.IfExpression:
            case SyntaxKind.MatchExpression:
            case SyntaxKind.LoopExpression:
            case SyntaxKind.WhileExpression:
            case SyntaxKind.ForeachExpression:
            case SyntaxKind.AssignmentExpression:
            case SyntaxKind.CallExpression:
                break;
            default:
                Messages.Add(CompilerMessage.Analyzers.InvalidExpressionForStatement(
                    node.Expression
                ));
                break;
        }
    }
}
