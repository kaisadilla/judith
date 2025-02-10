using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace Judith.NET.ext;

public static class CollectionExtension {
    /// <summary>
    /// Adds all the elements given to this collection.
    /// </summary>
    /// <param name="elements">All the elements to add.</param>
    public static void Add<T> (this ICollection<T> col, params T[] elements) {
        foreach (var element in elements) {
            col.Add(element);
        }
    }
}
