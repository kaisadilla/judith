using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET;

public readonly struct Version (int major, int minor, int patch, int build) {
    public int Major { get; init; } = major;
    public int Minor { get; init; } = minor;
    public int Patch { get; init; } = patch;
    public int Build { get; init; } = build;
}
