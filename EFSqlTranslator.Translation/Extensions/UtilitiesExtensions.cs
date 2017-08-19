using System;
using System.Collections.Generic;
using System.Linq;

namespace EFSqlTranslator.Translation.Extensions
{
    public static class UtilitiesExtensions
    {
        public static bool In<T>(this T item, IEnumerable<T> array)
        {
            return array.Contains(item);
        }
        
        public static bool In<T>(this T item, params T[] array)
        {
            return array.Contains(item);
        }

        public static TR GetOrAdd<TK, TR>(this IDictionary<TK, TR> dict, TK key, Func<TR> func) 
        {
            TR result;
            return dict.TryGetValue(key, out result) ? result : dict[key] = func();
        }
    }
}