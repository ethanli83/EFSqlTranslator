using EFSqlTranslator.Translation.DbObjects.SqlObjects;

namespace EFSqlTranslator.Translation.DbObjects.SqliteObjects
{
    public class SqliteObjectFactory : SqlObjectFactory
    {
        public override IDbColumn BuildColumn(
            DbReference dbRef, string colName, DbType type, string alias = null, bool isJoinKey = false)
        {
            var column = base.BuildColumn(dbRef, colName, type, alias, isJoinKey);
            column.OutputOption.QuotationMark = "'";
            return column;
        }

        public override IDbStatment BuildStatement(IDbObject script)
        {
            return new SqliteStatement(script);
        }
    }
}