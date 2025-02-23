using Judith.NET.analysis;
using Judith.NET.analysis.binder;
using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Judith.NET.diagnostics;

public class AstWithSemanticsPrinter : SyntaxVisitor<object> {
    private Compilation _cmp;

    public AstWithSemanticsPrinter (Compilation cmp) {
        _cmp = cmp;
    }

    public override object Visit (CompilerUnit node) {
        return new {
            Name = nameof(CompilerUnit),
            TopLevelItems = node.TopLevelItems.Select(t => Visit(t)),
            ImplicitFunction = VisitIfNotNull(node.ImplicitFunction),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (FunctionDefinition node) {
        return new {
            Name = nameof(FunctionDefinition),
            node.IsImplicit,
            node.IsHidden,
            Identifier = Visit(node.Identifier),
            Parameters = Visit(node.Parameters),
            ReturnTypeAnnotation = VisitIfNotNull(node.ReturnTypeAnnotation),
            Body = Visit(node.Body),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (StructTypeDefinition node) {
        return new {
            Name = nameof(StructTypeDefinition),
            node.IsHidden,
            Identifier = Visit(node.Identifier),
            MemberFields = node.MemberFields.Select(f => Visit(f)),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (BlockStatement node) {
        return new {
            Name = nameof(BlockStatement),
            Nodes = node.Nodes.Select(n => Visit(n)),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (ArrowStatement node) {
        return new {
            Name = nameof(ArrowStatement),
            Statement = Visit(node.Statement),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (LocalDeclarationStatement node) {
        return new {
            Name = nameof(LocalDeclarationStatement),
            DeclaratorList = Visit(node.DeclaratorList),
            Initializer = VisitIfNotNull(node.Initializer),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (ReturnStatement node) {
        return new {
            Name = nameof(ReturnStatement),
            Expression = VisitIfNotNull(node.Expression),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (YieldStatement node) {
        return new {
            Name = nameof(YieldStatement),
            Expression = Visit(node.Expression),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (WhenStatement node) {
        return new {
            Name = nameof(WhenStatement),
            Test = Visit(node.Test),
            Statement = Visit(node.Statement),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (ExpressionStatement node) {
        return new {
            Name = nameof(ExpressionStatement),
            Expression = Visit(node.Expression),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (IfExpression node) {
        return new {
            Name = nameof(IfExpression),
            Test = Visit(node.Test),
            Consequent = Visit(node.Consequent),
            Alternate = VisitIfNotNull(node.Alternate),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (MatchExpression node) {
        return new {
            Name = nameof(MatchExpression),
            Discriminant = Visit(node.Discriminant),
            Cases = node.Cases.Select(c => Visit(c)),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (LoopExpression node) {
        return new {
            Name = nameof(LoopExpression),
            Body = Visit(node.Body),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (WhileExpression node) {
        return new {
            Name = nameof(WhileExpression),
            Test = Visit(node.Test),
            Body = Visit(node.Body),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (ForeachExpression node) {
        return new {
            Name = nameof(ForeachExpression),
            Declarators = node.Declarators.Select(d => Visit(d)),
            Enumerable = Visit(node.Enumerable),
            Body = Visit(node.Body),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (AssignmentExpression node) {
        return new {
            Name = nameof(AssignmentExpression),
            Operator = Visit(node.Operator),
            Left = Visit(node.Left),
            Right = Visit(node.Right),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (BinaryExpression node) {
        return new {
            Name = nameof(BinaryExpression),
            Operator = Visit(node.Operator),
            Left = Visit(node.Left),
            Right = Visit(node.Right),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (LeftUnaryExpression node) {
        return new {
            Name = nameof(LeftUnaryExpression),
            Operator = Visit(node.Operator),
            Expression = Visit(node.Expression),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (CallExpression node) {
        return new {
            Callee = Visit(node.Callee),
            Arguments = Visit(node.Arguments),
        };
    }

    public override object Visit (AccessExpression node) {
        return new {
            Name = nameof(AccessExpression),
            Operator = Visit(node.Operator),
            Left = Visit(node.Left),
            Right = Visit(node.Right),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (GroupExpression node) {
        return new {
            Name = nameof(GroupExpression),
            Expression = Visit(node.Expression),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (IdentifierExpression node) {
        return new {
            Name = nameof(IdentifierExpression),
            Identifier = Visit(node.Identifier),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (LiteralExpression node) {
        return new {
            Name = nameof(LiteralExpression),
            Literal = Visit(node.Literal),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (Identifier node) {
        return new {
            Name = nameof(Identifier),
            IdentifierName = node.Name,
            node.IsEscaped,
            node.IsMetaName,
            Source = node.RawToken?.Lexeme,
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (Literal node) {
        return new {
            Name = nameof(Literal),
            node.TokenKind,
            node.Source,
            OriginalSource = node.RawToken?.Lexeme,
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (LocalDeclaratorList node) {
        return new {
            Name = nameof(LocalDeclaratorList),
            node.DeclaratorKind,
            Declarators = node.Declarators.Select(d => Visit(d)),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (LocalDeclarator node) {
        return new {
            Name = nameof(LocalDeclarator),
            Identifier = Visit(node.Identifier),
            node.LocalKind,
            TypeAnnotation = VisitIfNotNull(node.TypeAnnotation),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (EqualsValueClause node) {
        return new {
            Name = nameof(EqualsValueClause),
            Value = Visit(node.Value),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (TypeAnnotation node) {
        return new {
            Name = nameof(TypeAnnotation),
            Identifier = Visit(node.Identifier),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (Operator node) {
        return new {
            Name = nameof(Operator),
            node.OperatorKind,
            Source = node.RawToken?.Lexeme,
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (ParameterList node) {
        return new {
            Name = nameof(ParameterList),
            Parameters = node.Parameters.Select(p => Visit(p)),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (Parameter node) {
        return new {
            Name = nameof(Parameter),
            Declarator = Visit(node.Declarator),
            DefaultValue = VisitIfNotNull(node.DefaultValue),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (ArgumentList node) {
        return new {
            Name = nameof(ArgumentList),
            Arguments = node.Arguments.Select(a => Visit(a)),
        };
    }

    public override object Visit (Argument node) {
        return new {
            Name = nameof(Argument),
            Expression = Visit(node.Expression),
        };
    }

    public override object Visit (MatchCase node) {
        return new {
            Name = nameof(MatchCase),
            node.IsElseCase,
            Tests = node.Tests.Select(t => Visit(t)),
            Consequent = Visit(node.Consequent),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (MemberField node) {
        return new {
            Name = nameof(MemberField),
            node.Access,
            node.IsStatic,
            node.IsMutable,
            Identifier = Visit(node.Identifier),
            TypeAnnotation = Visit(node.TypeAnnotation),
            Initializer = VisitIfNotNull(node.Initializer),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (P_PrintStatement node) {
        return new {
            Name = nameof(P_PrintStatement),
            Expression = Visit(node.Expression),
            Semantics = GetBoundOrNull(node),
        };
    }

    #region Helper methdods

    private object? GetBoundOrNull (SyntaxNode node) {
        if (_cmp.Binder.TryGetBoundNode(node, out BoundNode? boundNode) == false) {
            return null;
        }

        dynamic dyn = Newtonsoft.Json.JsonConvert.DeserializeObject(
            Newtonsoft.Json.JsonConvert.SerializeObject(boundNode)
        )!;

        dyn.Node = "<skipped>";

        return dyn;
    }
    #endregion
}
