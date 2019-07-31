using System;
using System.Collections.Generic;
using System.Linq;

namespace EFSqlTranslator.Translation.Extensions
{
    /// <summary> Provides a set of static methods for querying objects that implement <see cref="T:System.Collections.Generic.IEnumerable`1" /> </summary>
    public static class EnumerableExtensions
    {
        /// <summary> Returns a number that represents how many distinct elements in the specified sequence satisfy a condition </summary>
        /// <typeparam name="TSource"> The type of the elements of source </typeparam>
        /// <param name="source"> A sequence that contains elements to be counted </param>
        /// <param name="selector"> A function to test each element for a condition </param>
        /// <returns> A number that represents how many distinct elements in the sequence satisfy the condition in the predicate function </returns>
        public static int DistinctCount<TSource, TOut>(this IEnumerable<TSource> source, Func<TSource, TOut> selector)
        {
            return source.Select(selector).Distinct().Count();
        }
    }
}
