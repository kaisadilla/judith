using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jal;

public enum JalValueType : byte {
    Float64
}

public class JalValue {
    public JalValueType Type { get; set; }
}

public class JalValue<T> : JalValue {
    public T Value { get; set; }

    public JalValue (JalValueType type, T value) {
        Type = type;
        Value = value;
    }
}

