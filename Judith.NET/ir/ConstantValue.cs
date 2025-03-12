using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.ir;

public enum ConstantValueKind {
    Integer,
    UnsignedInteger,
    Float,
    Boolean,
    String,
}

public class ConstantValue {
    public ConstantValueKind Kind { get; set; }

    public long AsInteger { get; private set; }
    public ulong AsUnsignedInteger { get; private set; }
    public double AsFloat { get; private set; }
    public bool AsBoolean { get; private set; }
    public string? AsString { get; private set; }

    public ConstantValue (long i64) {
        Kind = ConstantValueKind.Integer;
        AsInteger = i64;
    }

    public ConstantValue (ulong ui64) {
        Kind = ConstantValueKind.UnsignedInteger;
        AsUnsignedInteger = ui64;
    }

    public ConstantValue (double f64) {
        Kind = ConstantValueKind.Float;
        AsFloat = f64;
    }

    public ConstantValue (bool b) {
        Kind = ConstantValueKind.Boolean;
        AsBoolean = b;
    }

    public ConstantValue (string str) {
        Kind = ConstantValueKind.String;
        AsString = str;
    }
}
