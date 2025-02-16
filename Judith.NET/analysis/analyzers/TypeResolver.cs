using Judith.NET.analysis.syntax;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.analyzers;

/// <summary>
/// Traverses all the nodes in a compilation unit, resolving the type of every
/// expression that has a type.
/// </summary>
public class TypeResolver : SyntaxVisitor {
    public MessageContainer Messages { get; private set; } = new();

    private Compilation _program;

    public TypeResolver (Compilation program) {
        _program = program;
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    public override void Visit (FunctionDefinition node) {
        throw new NotImplementedException(); // TODO.
    }

    public override void Visit (LocalDeclarationStatement node) {
        if (node.Initializer != null) {
            Visit(node.Initializer);
        }

        ResolveType(node.DeclaratorList, node.Initializer?.Value.Type);
    }

    public override void Visit (ReturnStatement node) {
        if (node.Expression != null) {
            Visit(node.Expression);
        }
    }

    public override void Visit (YieldStatement node) {
        if (node.Expression != null) {
            Visit(node.Expression);
        }
    }

    public override void Visit (ExpressionStatement node) {
        if (node.Expression != null) {
            Visit(node.Expression);
        }
    }

    // if, match, loop, while, foreach

    public override void Visit (AssignmentExpression node) {
        Visit(node.Left);
        Visit(node.Right);

        if (node.Right.Type == null) {
            throw new Exception("AssignmentExpression.right didn't resolve to any type!");
        }

        node.SetType(node.Right.Type);
    }

    public override void Visit (BinaryExpression node) {
        Visit(node.Left);
        Visit(node.Right);

        if (node.Right.Type == null) {
            throw new Exception("BinaryExpression.right didn't resolve to any type!");
        }

        node.SetType(node.Right.Type);
    }

    public override void Visit (LeftUnaryExpression node) {
        Visit(node.Expression);

        if (node.Expression.Type == null) {
            throw new Exception("LeftUnaryExpression.Expression didn't resolve to any type!");
        }

        node.SetType(node.Expression.Type);
    }

    // TODO: AccessExpression

    public override void Visit (GroupExpression node) {
        Visit(node.Expression);

        if (node.Expression.Type == null) {
            throw new Exception("GroupExpression.Expression didn't resolve to any type!");
        }

        node.SetType(node.Expression.Type);
    }

    public override void Visit (LocalDeclaratorList node) {}

    public void ResolveType (LocalDeclaratorList node, TypeInfo? inferredType) {
        // Calculate type starting from the last. Type may be inferred from the
        // equals value clause, if it exists. Note that an explicit type
        // anotation always has precedence over inferred type. Also note that,
        // due to the syntax of local declaration in Judith, type annotations
        // also apply to every local listed before it, up until the last local
        // that has its own annotation (e.g. in "const a, b: Num, c, d: String",
        // "String" applies to both "d" and "c"). For this reason, we start
        // calculating type from last to first.
        for (int i = node.Declarators.Count - 1; i >= 0; i--) {
            ResolveType(node.Declarators[i], inferredType);
            // The type calculated for the last variable becomes the new
            // inferred type.
            inferredType = node.Declarators[i].Type;
        }
    }

    public override void Visit (LocalDeclarator node) { }

    public void ResolveType (LocalDeclarator node, TypeInfo? inferredType) {
        // If the type has no type annotation, then we have two options: if
        // type can be inferred, then it is inferred. If it can't, then the
        // type is left unresolved (which will likely be a compile-time error
        // in later analyses).
        if (node.TypeAnnotation == null) {
            if (inferredType != null) {
                node.SetType(inferredType);
            }
            return;
        }

        // Prior symbol binding required.
        if (node.TypeAnnotation.Identifier.Symbol == null) {
            throw new Exception("Identifier is not resolved.");
        }

        string typeName = node.TypeAnnotation.Identifier.Symbol.FullyQualifiedName;

        // If we can get the type the annotation refers to, then that's the type
        // of the declarator.
        if (_program.TypeTable.TryGetType(typeName, out TypeInfo? typeInfo)) {
            node.SetType(typeInfo);
        }
        // Otherwise, it's a compile-time error because the annotation points to
        // an unexistent type:
        else {
            Messages.Add(CompilerMessage.Analyzers.TypeDoesntExist(
                typeName, node.TypeAnnotation.Line
            ));
        }
    }

    public override void Visit (EqualsValueClause node) {
        Visit(node.Value);
    }
}
