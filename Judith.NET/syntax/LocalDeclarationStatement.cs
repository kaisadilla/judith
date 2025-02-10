﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;

public class LocalDeclarationStatement : Statement {
    public FieldDeclarationExpression Declaration { get; init; }

    public LocalDeclarationStatement (FieldDeclarationExpression declaration)
        : base(SyntaxKind.LocalDeclarationStatement)
    {
        Declaration = declaration;

        Children.Add(Declaration);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return $"|local_decl> {Declaration} <|";
    }
}
