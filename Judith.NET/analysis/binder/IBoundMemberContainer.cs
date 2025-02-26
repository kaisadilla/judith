using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.binder;

/// <summary>
/// Represents a bound node that contains member fields and methods.
/// </summary>
public interface IBoundMemberContainer {
    List<MemberDescription> Members { get; }

    public void AddMember (MemberDescription member);
}
