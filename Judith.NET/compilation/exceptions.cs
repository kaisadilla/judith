using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.compilation;

public class InvalidStepException : Exception {
    public InvalidStepException(string message) : base(message) { }
}
