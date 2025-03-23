using Judith.NET.analysis.syntax;
using Judith.NET.ir.syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.debugging;

public class DebuggingInfo {
    /// <summary>
    /// Maps each IR node to the Judith node that originated it.
    /// </summary>
    public Dictionary<IRNode, SyntaxNode> IRNodeMap { get; private set; } = [];
}
