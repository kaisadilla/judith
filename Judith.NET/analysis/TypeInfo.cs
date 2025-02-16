using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis;

public class TypeInfo {
    public string Name { get; set; }
    public string FullyQualifiedName { get; set; }

    public TypeInfo (string name, string fullyQualifiedName) {
        Name = name;
        FullyQualifiedName = fullyQualifiedName;
    }
}
