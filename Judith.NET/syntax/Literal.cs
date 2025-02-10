using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.syntax;
public class Literal : SyntaxNode {
    public LiteralValue? Value { get; private set; }
    public LiteralKind LiteralKind { get; init; }

    public Token? RawToken { get; init; }


    private Literal () : base(SyntaxKind.Literal) { }

    public Literal (LiteralKind kind) : this () {
        LiteralKind = kind;
    }

    public void SetValue (double value) {
        Value = new FloatValue(value);
    }

    public void SetValue (long value) {
        Value = new IntegerValue(value);
    }

    public void SetValue (bool value) {
        Value = new BooleanValue(value);
    }

    public override void Accept (SyntaxVisitor visitor) {
        visitor.Visit(this);
    }

    public override string ToString () {
        return $"{RawToken?.Lexeme ?? "<unknown literal>"}";
    }
}

public abstract class LiteralValue {

}

public class BooleanValue : LiteralValue {
    public bool Value { get; init; }

    public BooleanValue (bool value) {
        Value = value;
    }
}

public class FloatValue : LiteralValue {
    public double Value { get; init; }

    public FloatValue (double value) {
        Value = value;
    }
}

public class IntegerValue : LiteralValue {
    public long Value { get; init; }

    public IntegerValue (long value) {
        Value = value;
    }
}
