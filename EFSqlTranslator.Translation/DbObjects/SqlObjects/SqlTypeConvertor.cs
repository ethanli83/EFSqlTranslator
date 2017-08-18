using System;
using System.Linq;
using System.Reflection;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlTypeConvertor
    {
        public string Convert(Type type)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var gType = type.GenericTypeArguments.Single();
                return Convert(gType);
            }

            if (type == typeof(bool))
                return "bit";

            if (type == typeof(int))
                return "int";

            if (type == typeof(string))
                return "nvarchar";

            if (type == typeof(double) || type == typeof(decimal) || type == typeof(float))
                return "decimal";

            if (type == typeof(Guid))
                return "uniqueidentifier";

            if (type == typeof(DateTime))
                return "datetime";

            if (type == typeof(DbJoinType))
                return "<<DbJoinType>>";
            
            if (type.IsArray)
                return "<<Array>>";
            
            if (type.IsEnumerable())
                return "<<Enumerable>>";

            throw new NotImplementedException($"{type.Name} not supported.");
        }
    }
}