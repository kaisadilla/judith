using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.syntax;

public class ObjectInitializationExpression {
    public Expression? Provider { get; private set; }
    public ObjectInitializer Initializer { get; private set; }
}
