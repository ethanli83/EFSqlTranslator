using EFSqlTranslator.Translation.DbObjects.SqlObjects;

namespace EFSqlTranslator.Translation.DbObjects.SqliteObjects
{
    public class SqliteObjectFactory : SqlObjectFactory
    {
        public override IDbFunc BuildNullCheckFunc(params IDbObject[] parameters)
        {
            return new SqlFunc("ifnull", parameters) { IsAggregation = true };
        }
    }
}