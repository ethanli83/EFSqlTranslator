using System;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;

namespace EFSqlTranslator.Translation.DbObjects.SqliteObjects
{
    public class SqliteFunc : SqlFunc
    {
        public SqliteFunc(string name, Type type, IDbObject[] parameters) : base(name, type, parameters)
        {
        }

        public override string ToString()
        {
            var name = Name.ToLower();
            var requireCastToReal = IsAggregation && (name == "sum" || name == "average");
            return $"{base.ToString()}" + (requireCastToReal ? " * 1.0" : string.Empty);
        }
    }
}