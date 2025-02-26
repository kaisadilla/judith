namespace Judith.NET.analysis.syntax;

public class ObjectInitializer : SyntaxNode {
    public List<FieldInitialization> FieldInitializations { get; private init; }

    public Token? LeftBracketToken { get; set; }
    public Token? RightBracketToken { get; set; }

    public ObjectInitializer (List<FieldInitialization> fieldInits)
        : base(SyntaxKind.ObjectInitializer)
    {
        FieldInitializations = fieldInits;

        Children.AddRange(FieldInitializations);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override T? Accept<T> (SyntaxVisitor<T> visitor) where T : default {
        return visitor.Visit(this);
    }
}

