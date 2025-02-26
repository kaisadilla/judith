using Judith.NET.analysis.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Judith.NET.diagnostics;

public class SimpleAstPrinter : SyntaxVisitor<List<string>> {
    public override List<string> Visit (CompilerUnit node) {
        List<string> txt = [];

        List<List<string>> topLevelItems = [];
        List<string>? implicitFunc = null;

        foreach (var i in node.TopLevelItems) {
            AddIfNotNull(topLevelItems, Visit(i));
        }

        if (node.ImplicitFunction != null) {
            implicitFunc = Visit(node.ImplicitFunction);
        }

        txt.Add("compiler_unit {");
        if (implicitFunc == null) {
            txt.Add("    implicit_func: null");
        }
        else {
            txt.Add("    implicit_func: " + implicitFunc[0]);
            foreach (var str in implicitFunc[1..]) {
                txt.Add("    " + str);
            }
        }
        txt[^1] += ",";
        txt.Add("    top_level_items: [");
        foreach (var strArray in topLevelItems) {
            AddNewline(txt, strArray, 2);
        }
        txt.Add("    ]");
        txt.Add("}");

        return txt;
    }

    public override List<string> Visit (FunctionDefinition node) {
        List<string> txt = [];

        List<string> parameters = Visit(node.Parameters);
        List<string> body = Visit(node.Body);

        txt.Add($"function_definition (name: {node.Identifier.Name}, implicit: {node.IsImplicit}, hidden: {node.IsHidden}) {{");
        txt.Add($"    parameters (count: {node.Parameters.Parameters.Count}) [");
        foreach (var p in parameters) {
            txt.Add("        " + p);
        }
        txt.Add("    ],");
        if (node.ReturnTypeAnnotation != null) {
            txt.Add("    type_annotation: " + Visit(node.ReturnTypeAnnotation) + ",");
        }
        else {
            txt.Add("    type_annotation: null,");
        }
        txt.Add("    body [");
        foreach (var b in body) {
            txt.Add("        " + b);
        }
        txt.Add("    ]");
        txt.Add("}");

        return txt;
    }

    public override List<string>? Visit (StructTypeDefinition node) {
        List<string> txt = [];

        txt.Add($"typedef struct (name: {node.Identifier.Name}) {{");

        foreach (var member in node.MemberFields) {
            AddNewline(txt, Visit(member), 1);
        }

        txt.Add("}");

        return txt;
    }

    public override List<string> Visit (BlockStatement node) {
        List<string> txt = [];

        foreach (var n in node.Nodes) {
            txt.AddRange(Visit(n));
        }

        return txt;
    }

    public override List<string> Visit (ArrowStatement node) {
        List<string> txt = [];

        List<string> expr = Visit(node.Statement);

        txt.Add("=> " + expr[0]);
        if (expr.Count > 1) {
            txt.AddRange(expr[1..]);
        }

        return txt;
    }

    public override List<string> Visit (LocalDeclarationStatement node) {
        List<string> txt = [];

        List<string> declarators = Visit(node.DeclaratorList);

        txt.Add("local_decl_stmt { ");
        AddInline(txt, declarators, 1);

        if (node.Initializer != null) {
            AddInline(txt, Visit(node.Initializer), 1);
        }

        txt[0] += " }";

        return txt;
    }

    public override List<string> Visit (ReturnStatement node) {
        if (node.Expression == null) {
            return ["return"];
        }

        List<string> txt = [];

        List<string> expr = Visit(node.Expression);

        txt.Add("return { " + expr[0]);
        if (expr.Count > 1) {
            txt.AddRange(expr[1..]);
        }
        txt[^1] += " }";

        return txt;
    }

    public override List<string> Visit (YieldStatement node) {
        List<string> txt = [];

        List<string> expr = Visit(node.Expression);

        txt.Add("yield { " + expr[0]);
        if (expr.Count > 1) {
            txt.AddRange(expr[1..]);
        }
        txt[^1] += " }";

        return txt;
    }

    public override List<string> Visit (ExpressionStatement node) {
        List<string> txt = [""];
        
        AddInline(txt, Visit(node.Expression), 1);
        txt[^1] += ";";

        return txt;
    }

    public override List<string> Visit (IfExpression node) {
        List<string> txt = [];

        txt.Add("if {");
        txt.Add("    test: ");
        AddInline(txt, Visit(node.Test), 2);
        txt[^1] += ",";
        txt.Add("    consequent: ");
        AddInline(txt, Visit(node.Consequent), 2);

        if (node.Alternate != null) {
            txt[^1] += ",";
            txt.Add("    alternate: ");
            AddInline(txt, Visit(node.Alternate), 2);
        }
        txt.Add("}");

        return txt;
    }

    public override List<string> Visit (MatchExpression node) {
        List<string> txt = [];

        txt.Add("match {");
        txt.Add("    cases: [");

        foreach (var c in node.Cases) {
            AddNewline(txt, Visit(c), 2);
        }
        txt.Add("}");

        return txt;
    }

    public override List<string> Visit (LoopExpression node) {
        List<string> txt = ["loop {"];

        AddNewline(txt, Visit(node.Body), 1);
        txt.Add("}");

        return txt;
    }

    public override List<string> Visit (WhileExpression node) {
        List<string> txt = ["while {"];

        txt.Add("    test: ");
        AddInline(txt, Visit(node.Test), 2);
        txt[^1] += ",";
        txt.Add("    body: {");
        AddNewline(txt, Visit(node.Body), 2);
        txt.Add("    }");
        txt.Add("}");

        return txt;
    }

    public override List<string> Visit (ForeachExpression node) {
        List<string> txt = [];

        List<string> localDecls = [];
        foreach (var localDecl in node.Declarators) {
            localDecls.AddRange(Visit(localDecl));
        }

        List<string> enumerable = Visit(node.Enumerable);

        txt.Add("foreach {");

        txt.Add("    declarators: [");
        AddNewline(txt, localDecls, 2);
        txt[^1] += "],";
        txt.Add("    in: ");
        AddInline(txt, enumerable, 2);
        txt[^1] += ",";
        txt.Add("    body: {");
        AddNewline(txt, Visit(node.Body), 2);
        txt.Add("    }");
        txt.Add("}");

        return txt;
    }

    public override List<string> Visit (AssignmentExpression node) {
        List<string> txt = [""];

        AddInline(txt, Visit(node.Left), 1);
        txt[^1] += " = ";
        AddInline(txt, Visit(node.Right), 1);

        return txt;
    }

    public override List<string> Visit (BinaryExpression node) {
        List<string> txt = ["("];

        AddInline(txt, Visit(node.Left), 1);
        txt[^1] += $" {Visit(node.Operator)[0]} ";
        AddInline(txt, Visit(node.Right), 1);
        txt[^1] += ")";

        return txt;
    }

    public override List<string> Visit (LeftUnaryExpression node) {
        List<string> txt = [""];

        txt[^1] += $" {Visit(node.Operator)[0]} ";
        AddInline(txt, Visit(node.Expression), 1);

        return txt;
    }

    public override List<string>? Visit (ObjectInitializationExpression node) {
        List<string> txt = ["object_init_expr: {"];

        txt.Add("    provider: ");
        AddInline(txt, VisitIfNotNull(node.Provider) ?? ["null"], 1);

        txt.Add("    initializer: {");
        AddInline(txt, Visit(node.Initializer), 1);
        txt[^1] += "}";
        txt.Add("}");

        return txt;
    }

    public override List<string>? Visit (CallExpression node) {
        List<string> txt = [""];

        AddInline(txt, Visit(node.Callee), 1);
        AddInline(txt, Visit(node.Arguments), 0);

        return txt;
    }

    public override List<string> Visit (AccessExpression node) {
        List<string> txt = ["("];

        AddInline(txt, Visit(node.Receiver), 1);
        txt[^1] += $" {Visit(node.Operator)[0]} ";
        AddInline(txt, Visit(node.Member), 1);
        txt[^1] += ")";

        return txt;
    }

    public override List<string> Visit (GroupExpression node) {
        List<string> txt = ["("];

        AddInline(txt, Visit(node.Expression), 1);
        txt[^1] += ")";

        return txt;
    }

    public override List<string> Visit (IdentifierExpression node) {
        return Visit(node.Identifier);
    }

    public override List<string> Visit (LiteralExpression node) {
        return Visit(node.Literal);
    }

    public override List<string> Visit (Identifier node) {
        List<string> txt = [node.Name];
        if (node.IsMetaName) {
            txt[^1] += " (metaname)";
        }

        return txt;
    }
    
    public override List<string> Visit (Literal node) {
        return [node.Source];
    }
    
    public override List<string> Visit (LocalDeclaratorList node) {
        List<string> txt = [""];

        List<string> declarators = [];
        foreach (var decl in node.Declarators) {
            declarators.AddRange(Visit(decl));
        }

        foreach (var decl in declarators) {
            txt[0] += decl + ", ";
        }
        txt[0] = txt[0][..^2];

        return txt;
    }
    
    public override List<string> Visit (LocalDeclarator node) {
        List<string> txt = [];

        txt.Add(node.LocalKind == LocalKind.Constant ? "[const] " : "[var] ");
        txt[^1] += node.Identifier.Name;
        if (node.TypeAnnotation != null) {
            AddInline(txt, Visit(node.TypeAnnotation), 1);
        }

        return txt;
    }

    public override List<string> Visit (EqualsValueClause node) {
        List<string> txt = [" = "];

        AddInline(txt, Visit(node.Value), 1);

        return txt;
    }

    public override List<string> Visit (TypeAnnotation node) {
        List<string> txt = [": "];

        AddInline(txt, Visit(node.Identifier), 1);

        return txt;
    }

    public override List<string> Visit (Operator node) {
        List<string> txt = [node.RawToken?.Lexeme ?? $"({node.OperatorKind})"];

        return txt;
    }

    public override List<string> Visit (ParameterList node) {
        List<string> txt = [];

        foreach (var param in node.Parameters) {
            txt.AddRange(Visit(param));
        }

        return txt;
    }

    public override List<string> Visit (Parameter node) {
        List<string> txt = [""];

        AddInline(txt, Visit(node.Declarator), 1);
        if (node.DefaultValue != null) {
            AddInline(txt, Visit(node.DefaultValue), 1);
        }

        return txt;
    }

    public override List<string> Visit (ArgumentList node) {
        List<string> txt = ["("];

        foreach (var arg in node.Arguments) {
            AddInline(txt, Visit(arg), 1);
            txt[^1] += ", ";
        }

        txt[^1] += ")";

        return txt;
    }

    public override List<string> Visit (Argument node) {
        List<string> txt = [""];

        AddInline(txt, Visit(node.Expression), 1);

        return txt;
    }

    public override List<string> Visit (MatchCase node) {
        List<string> txt = [$"case (is_else: {node.IsElseCase}) {{"];

        txt.Add("    tests: [");
        foreach (var test in node.Tests) {
            AddNewline(txt, Visit(test), 2);
        }
        txt.Add("    ],");
        txt.Add("    consequent: {");
        AddNewline(txt, Visit(node.Consequent), 2);
        txt.Add("    ]");
        txt.Add("}");

        return txt;
    }

    public override List<string>? Visit (ObjectInitializer node) {
        List<string> txt = ["["];

        foreach (var fieldInit in node.FieldInitializations) {
            AddNewline(txt, Visit(fieldInit), 1);
        }

        txt.Add("]");

        return txt;
    }

    public override List<string>? Visit (FieldInitialization node) {
        List<string> txt = [$"{node.FieldName.Name} = "];

        AddInline(txt, Visit(node.Initializer), 1);

        return txt;
    }

    public override List<string>? Visit (MemberField node) {
        List<string> txt = [$"member_field (name: {node.Identifier.Name}, " +
            $"access: {node.Access}, isStatic: {node.IsStatic}, isMutable: " +
            $"{node.IsMutable}, type"
        ];

        AddInline(txt, Visit(node.TypeAnnotation), 1);

        if (node.Initializer != null) {
            txt[^1] += ") = ";
            AddInline(txt, Visit(node.Initializer), 1);
        }
        else {
            txt[^1] += ")";
        }

        return txt;
    }

    public override List<string> Visit (P_PrintStatement node) {
        List<string> txt = ["__p_print ("];

        AddInline(txt, Visit(node.Expression), 1);
        txt[^1] += ")";

        return txt;
    }

    private void AddIfNotNull<T> (List<T> arr, T? toAdd) {
        if (toAdd != null) arr.Add(toAdd);
    }

    private void AddInline (List<string> arr, List<string>? toAdd, int indent) {
        if (toAdd == null) return;

        arr[^1] += toAdd[0];
        if (toAdd.Count > 1) {
            foreach (var str in toAdd[1..]) {
                arr.Add(Spaces(indent * 4) + str);
            }
        }
    }

    private void AddNewline (List<string> arr, List<string>? toAdd, int indent) {
        if (toAdd == null) return;

        foreach (var str in toAdd) {
            arr.Add(Spaces(indent * 4) + str);
        }
    }

    private string Spaces (int count) {
        string str = "";
        for (int i = 0; i < count; i++) {
            str += " ";
        }
        return str;
    }
}
