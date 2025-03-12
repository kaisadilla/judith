using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.codegen;

public class InvalidIRProgramException : Exception {
    public InvalidIRProgramException (string msg) : base (msg) { }
}
