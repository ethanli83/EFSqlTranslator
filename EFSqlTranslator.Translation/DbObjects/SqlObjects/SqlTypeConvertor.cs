using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlTypeConvertor
    {
        public DbType Convert(Type type)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var gType = type.GenericTypeArguments.Single();
                return Convert(gType);
            }

            if (type == typeof(bool))
                return DbType.Boolean;

            if (type == typeof(short))
                return DbType.Int16;
            
            if (type == typeof(int))
                return DbType.Int32;
            
            if (type == typeof(long))
                return DbType.Int64;

            if (type == typeof(string))
                return DbType.String;

            if (type == typeof(double))
                return DbType.Double;
            
            if (type == typeof(decimal) || type == typeof(float))
                return DbType.Decimal;

            if (type == typeof(Guid))
                return DbType.Guid;

            if (type == typeof(DateTime))
                return DbType.DateTime;

            if (type == typeof(DbJoinType) || type.IsEnumerable() || type.IsArray)
                return DbType.Object;
            
            throw new NotImplementedException($"{type.Name} not supported.");
        }
    }
}