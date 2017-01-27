using System.Collections.Generic;
using System.Linq;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation
{
    public static class DbObjectExtensions
    {
        public static T[] GetDbObjects<T>(this IDbObject dbObject)
        {
            var result = new List<T>();
            if (dbObject is T)
                result.Add((T)dbObject);

            var dbFunc = dbObject as IDbFunc;
            if (dbFunc != null)
            {
                result.AddRange(dbFunc.Parameters.SelectMany(p => GetDbObjects<T>(p)));
            }

            var dbBinary = dbObject as IDbBinary;
            if (dbBinary != null)
            {
                result.AddRange(dbBinary.Left.GetDbObjects<T>());
                result.AddRange(dbBinary.Right.GetDbObjects<T>());
            }

            var dbCondition = dbObject as IDbCondition;
            if (dbCondition != null)
            {
                var conditions = dbCondition.Conditions.
                    SelectMany(c => c.Item1.GetDbObjects<T>().Concat(c.Item2.GetDbObjects<T>()));

                result.AddRange(conditions);
                result.AddRange(dbCondition.Else.GetDbObjects<T>());
            }
            
            return result.ToArray();
        }
    }
}