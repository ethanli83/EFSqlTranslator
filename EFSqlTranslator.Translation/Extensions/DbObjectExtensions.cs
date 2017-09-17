using System.Collections.Generic;
using System.Data;
using System.Linq;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation.Extensions
{
    public static class DbObjectExtensions
    {
        public static T[] GetDbObjects<T>(this IDbObject dbObject)
        {
            var result = new List<T>();
            if (dbObject is T variable)
                result.Add(variable);

            switch (dbObject)
            {
                case IDbScript dbScript:
                    result.AddRange(dbScript.PreScripts.SelectMany(GetDbObjects<T>));
                    result.AddRange(dbScript.Scripts.SelectMany(GetDbObjects<T>));
                    result.AddRange(dbScript.PostScripts.SelectMany(GetDbObjects<T>));
                    break;
                
                case IDbSelect dbSelect:
                    result.AddRange(dbSelect.Selection.SelectMany(GetDbObjects<T>));
                    result.AddRange(dbSelect.From.GetDbObjects<T>());
                    result.AddRange(dbSelect.Joins.SelectMany(GetDbObjects<T>));
                    result.AddRange(dbSelect.Where.GetDbObjects<T>());
                    result.AddRange(dbSelect.GroupBys.SelectMany(GetDbObjects<T>));
                    result.AddRange(dbSelect.OrderBys.SelectMany(GetDbObjects<T>));
                    break;
                    
                case IDbColumn dbColumn:
                    result.AddRange(dbColumn.Ref.GetDbObjects<T>());
                    break;
                    
                case IDbJoin dbJoin:
                    result.AddRange(dbJoin.Condition.GetDbObjects<T>());
                    result.AddRange(dbJoin.To.GetDbObjects<T>());
                    break;
                    
                case IDbFunc dbFunc:
                    result.AddRange(dbFunc.Parameters.SelectMany(GetDbObjects<T>));
                    break;
                    
                case IDbBinary dbBinary:
                    result.AddRange(dbBinary.Left.GetDbObjects<T>());
                    result.AddRange(dbBinary.Right.GetDbObjects<T>());
                    break;
                    
                case IDbCondition dbCondition:
                    var conditions = dbCondition.Conditions.
                        SelectMany(c => c.Item1.GetDbObjects<T>().Concat(c.Item2.GetDbObjects<T>()));

                    result.AddRange(conditions);
                    result.AddRange(dbCondition.Else.GetDbObjects<T>());
                    break;
                    
                case IDbSelectable dbSelectable:
                    result.AddRange(dbSelectable.Ref.GetDbObjects<T>());
                    break;
            }

            return result.ToArray();
        }
        
        /// <summary>
        /// Parameterise all the constants so that the query can be cached by ORM
        /// </summary>
        /// <param name="dbObj"></param>
        /// <param name="ignoreEnumerable">Set to true if does not want to parameterise array.
        /// This is required if ORM does not support passing array as parameter.</param>
        /// <returns></returns>
        public static IDbConstant[] Parameterise(this IDbObject dbObj, bool ignoreEnumerable = false)
        {
            var constants = dbObj.GetDbObjects<IDbConstant>().Where(c => c.AsParam).ToArray();
            if (ignoreEnumerable)
            {
                constants = constants.Where(c => !c.ValType.DotNetType.IsEnumerable()).ToArray();
            }
            
            var dict = new Dictionary<object, List<IDbConstant>>();
            foreach (var c in constants)
            {
                if (dict.ContainsKey(c.Val))
                {
                    dict[c.Val].Add(c);
                    continue;
                }
                
                dict[c.Val] = new List<IDbConstant>() { c };
            }
            
            var parameters = dict.Values.ToArray();
            for (var i = 0; i < parameters.Length; i++)
            {
                foreach (var c in parameters[i])
                {
                    c.ParamName = $"@param{i}";
                }
            }

            return parameters.Select(p => p.First()).ToArray();
        }
    }
}