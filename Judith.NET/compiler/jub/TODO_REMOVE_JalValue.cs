using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compiler.jub;

public enum JalValueType : byte {
    Float64
}

public class TODO_REMOVE_JalValue {
    public JalValueType Type { get; set; }
}

public class TODO_REMOVE_JalValue<T> : TODO_REMOVE_JalValue {
    public T Value { get; set; }

    public TODO_REMOVE_JalValue (JalValueType type, T value) {
        Type = type;
        Value = value;
    }
}

