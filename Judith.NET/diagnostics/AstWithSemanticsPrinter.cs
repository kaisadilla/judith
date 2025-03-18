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
    private JudithCompilation _cmp;

    public AstWithSemanticsPrinter (JudithCompilation cmp) {
        _cmp = cmp;
    }

    public override object Visit (CompilerUnit node) {
        return new {
            Class = nameof(CompilerUnit),
            node.Kind,
            TopLevelItems = node.TopLevelItems.Select(t => Visit(t)),
            ImplicitFunction = VisitIfNotNull(node.ImplicitFunction),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (FunctionDefinition node) {
        return new {
            Class = nameof(FunctionDefinition),
            node.Kind,
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
            Class = nameof(StructTypeDefinition),
            node.Kind,
            node.IsHidden,
            Identifier = Visit(node.Identifier),
            MemberFields = node.MemberFields.Select(f => Visit(f)),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (BlockStatement node) {
        return new {
            Class = nameof(BlockStatement),
            node.Kind,
            Nodes = node.Nodes.Select(n => Visit(n)),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (ArrowStatement node) {
        return new {
            Class = nameof(ArrowStatement),
            node.Kind,
            Statement = Visit(node.Statement),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (LocalDeclarationStatement node) {
        return new {
            Class = nameof(LocalDeclarationStatement),
            node.Kind,
            DeclaratorList = Visit(node.DeclaratorList),
            Initializer = VisitIfNotNull(node.Initializer),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (ReturnStatement node) {
        return new {
            Class = nameof(ReturnStatement),
            node.Kind,
            Expression = VisitIfNotNull(node.Expression),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (YieldStatement node) {
        return new {
            Class = nameof(YieldStatement),
            node.Kind,
            Expression = Visit(node.Expression),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (WhenStatement node) {
        return new {
            Class = nameof(WhenStatement),
            node.Kind,
            Test = Visit(node.Test),
            Statement = Visit(node.Statement),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (ExpressionStatement node) {
        return new {
            Class = nameof(ExpressionStatement),
            node.Kind,
            Expression = Visit(node.Expression),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (IfExpression node) {
        return new {
            Class = nameof(IfExpression),
            node.Kind,
            Test = Visit(node.Test),
            Consequent = Visit(node.Consequent),
            Alternate = VisitIfNotNull(node.Alternate),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (MatchExpression node) {
        return new {
            Class = nameof(MatchExpression),
            node.Kind,
            Discriminant = Visit(node.Discriminant),
            Cases = node.Cases.Select(c => Visit(c)),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (LoopExpression node) {
        return new {
            Class = nameof(LoopExpression),
            node.Kind,
            Body = Visit(node.Body),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (WhileExpression node) {
        return new {
            Class = nameof(WhileExpression),
            node.Kind,
            Test = Visit(node.Test),
            Body = Visit(node.Body),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (ForeachExpression node) {
        return new {
            Class = nameof(ForeachExpression),
            node.Kind,
            Declarators = node.Declarators.Select(d => Visit(d)),
            Enumerable = Visit(node.Enumerable),
            Body = Visit(node.Body),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (AssignmentExpression node) {
        return new {
            Class = nameof(AssignmentExpression),
            node.Kind,
            Operator = Visit(node.Operator),
            Left = Visit(node.Left),
            Right = Visit(node.Right),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (BinaryExpression node) {
        return new {
            Class = nameof(BinaryExpression),
            node.Kind,
            Operator = Visit(node.Operator),
            Left = Visit(node.Left),
            Right = Visit(node.Right),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (LeftUnaryExpression node) {
        return new {
            Class = nameof(LeftUnaryExpression),
            node.Kind,
            Operator = Visit(node.Operator),
            Expression = Visit(node.Expression),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (ObjectInitializationExpression node) {
        return new {
            Class = nameof(ObjectInitializationExpression),
            node.Kind,
            Provider = VisitIfNotNull(node.Provider),
            Initializer = Visit(node.Initializer),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (CallExpression node) {
        return new {
            Class = nameof(CallExpression),
            node.Kind,
            Callee = Visit(node.Callee),
            Arguments = Visit(node.Arguments),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (AccessExpression node) {
        return new {
            Class = nameof(AccessExpression),
            node.Kind,
            node.AccessKind,
            Receiver = VisitIfNotNull(node.Receiver),
            Member = Visit(node.Member),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (GroupExpression node) {
        return new {
            Class = nameof(GroupExpression),
            node.Kind,
            Expression = Visit(node.Expression),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (IdentifierExpression node) {
        return new {
            Class = nameof(IdentifierExpression),
            node.Kind,
            Identifier = Visit(node.Name),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (LiteralExpression node) {
        return new {
            Class = nameof(LiteralExpression),
            node.Kind,
            Literal = Visit(node.Literal),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (QualifiedIdentifier node) {
        return new {
            Class = nameof(QualifiedIdentifier),
            node.Kind,
            Qualifier = Visit(node.Qualifier),
            Operator = Visit(node.Operator),
            Name = Visit(node.Name),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (SimpleIdentifier node) {
        return new {
            Class = nameof(SimpleIdentifier),
            node.Kind,
            IdentifierName = node.Name,
            node.IsEscaped,
            node.IsMetaName,
            Source = node.RawToken?.Lexeme,
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (Literal node) {
        return new {
            Class = nameof(Literal),
            node.Kind,
            node.TokenKind,
            node.Source,
            OriginalSource = node.RawToken?.Lexeme,
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (LocalDeclaratorList node) {
        return new {
            Class = nameof(LocalDeclaratorList),
            node.Kind,
            node.DeclaratorKind,
            Declarators = node.Declarators.Select(d => Visit(d)),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (LocalDeclarator node) {
        return new {
            Class = nameof(LocalDeclarator),
            node.Kind,
            Identifier = Visit(node.Identifier),
            node.LocalKind,
            TypeAnnotation = VisitIfNotNull(node.TypeAnnotation),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (EqualsValueClause node) {
        return new {
            Class = nameof(EqualsValueClause),
            node.Kind,
            Value = Visit(node.Value),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (TypeAnnotation node) {
        return new {
            Class = nameof(TypeAnnotation),
            node.Kind,
            Identifier = Visit(node.Type),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (Operator node) {
        return new {
            Class = nameof(Operator),
            node.Kind,
            node.OperatorKind,
            Source = node.RawToken?.Lexeme,
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (ParameterList node) {
        return new {
            Class = nameof(ParameterList),
            node.Kind,
            Parameters = node.Parameters.Select(p => Visit(p)),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (Parameter node) {
        return new {
            Class = nameof(Parameter),
            node.Kind,
            Declarator = Visit(node.Declarator),
            DefaultValue = VisitIfNotNull(node.DefaultValue),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (ArgumentList node) {
        return new {
            Class = nameof(ArgumentList),
            node.Kind,
            Arguments = node.Arguments.Select(a => Visit(a)),
        };
    }

    public override object Visit (Argument node) {
        return new {
            Class = nameof(Argument),
            node.Kind,
            Expression = Visit(node.Expression),
        };
    }

    public override object Visit (MatchCase node) {
        return new {
            Class = nameof(MatchCase),
            node.Kind,
            node.IsElseCase,
            Tests = node.Tests.Select(t => Visit(t)),
            Consequent = Visit(node.Consequent),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (ObjectInitializer node) {
        return new {
            Class = nameof(ObjectInitializer),
            node.Kind,
            Assignments = node.FieldInitializations.Select(a => Visit(a)),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (FieldInitialization node) {
        return new {
            Class = nameof(FieldInitialization),
            node.Kind,
            FieldName = Visit(node.FieldName),
            Initializer = VisitIfNotNull(node.Initializer),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (MemberField node) {
        return new {
            Class = nameof(MemberField),
            node.Kind,
            node.Access,
            node.IsStatic,
            node.IsMutable,
            Identifier = Visit(node.Identifier),
            TypeAnnotation = Visit(node.TypeAnnotation),
            Initializer = VisitIfNotNull(node.Initializer),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (GroupType node) {
        return new {
            Class = nameof(GroupType),
            node.Kind,
            node.IsConstant,
            node.IsNullable,
            Type = Visit(node.Type),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (IdentifierType node) {
        return new {
            Class = nameof(IdentifierType),
            node.Kind,
            node.IsConstant,
            node.IsNullable,
            Name = Visit(node.Name),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (FunctionType node) {
        return new {
            Class = nameof(FunctionType),
            node.Kind,
            node.IsConstant,
            node.IsNullable,
            ParameterTypes = node.ParameterTypes.Select(p => Visit(p)),
            ReturnType = Visit(node.ReturnType),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (TupleArrayType node) {
        return new {
            Class = nameof(TupleArrayType),
            node.Kind,
            node.IsConstant,
            node.IsNullable,
            MemberTypes = node.MemberTypes.Select(p => Visit(p)),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (RawArrayType node) {
        return new {
            Class = nameof(RawArrayType),
            node.Kind,
            node.IsConstant,
            node.IsNullable,
            MemberType = Visit(node.MemberType),
            Length = Visit(node.Length),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (ObjectType node) {
        throw new NotImplementedException("Object type not yet supported.");
    }

    public override object? Visit (LiteralType node) {
        return new {
            Class = nameof(LiteralType),
            node.Kind,
            node.IsConstant,
            node.IsNullable,
            Literal = Visit(node.Literal),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object? Visit (UnionType node) {
        return new {
            Class = nameof(UnionType),
            node.Kind,
            node.IsConstant,
            node.IsNullable,
            MemberTypes = node.MemberTypes.Select(p => Visit(p)),
            Semantics = GetBoundOrNull(node),
        };
    }

    public override object Visit (P_PrintStatement node) {
        return new {
            Class = nameof(P_PrintStatement),
            node.Kind,
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
