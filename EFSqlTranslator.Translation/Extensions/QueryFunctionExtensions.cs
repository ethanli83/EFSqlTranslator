using System.Collections.Generic;
using System.Linq;

namespace EFSqlTranslator.Translation.Extensions
{
    public static class QueryFunctionExtensions
    {
        public static bool In<T>(this T item, IEnumerable<T> array)
        {
            return array.Contains(item);
        }
        
        public static bool In<T>(this T item, params T[] array)
        {
            return array.Contains(item);
        }
    }
}