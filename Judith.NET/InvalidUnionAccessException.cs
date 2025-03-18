using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET;

public class InvalidUnionAccessException : Exception {
    /// <summary>
    /// The name of the field that was being accessed.
    /// </summary>
    public string AccessedField { get; private set; }
    /// <summary>
    /// The name of the union type.
    /// </summary>
    public string UnionTypeName { get; private set; }
    /// <summary>
    /// The name of the type currently held by the union.
    /// </summary>
    public string CurrentUnionType { get; private set; }

    public InvalidUnionAccessException (
        string accessedField, string unionTypeName, string currentUnionType
    )
        : base($"Tried to access field '{accessedField}' in union " +
            $"'{unionTypeName}' of type '{currentUnionType}'")
    {
        AccessedField = accessedField;
        UnionTypeName = unionTypeName;
        CurrentUnionType = currentUnionType;
    }
}

public class InvalidUnionException : Exception {

}