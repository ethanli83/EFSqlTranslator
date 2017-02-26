using System.Linq;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;

namespace EFSqlTranslator.Translation.DbObjects.MySqlObjects
{
    public class MySqlObjectFactory : SqlObjectFactory
    {
        public override IDbColumn BuildColumn(
            DbReference dbRef, string colName, DbType type, string alias = null, bool isJoinKey = false)
        {
            var column = base.BuildColumn(dbRef, colName, type, alias, isJoinKey);
            column.OutputOption.QuotationMark = "`";
            return column;
        }

        public override IDbTempTable BuildTempTable(string tableName, IDbSelect sourceSelect = null)
        {
            var sqlTable = new MySqlTempTable
            {
                TableName = tableName,
                SourceSelect = sourceSelect,
                OutputOption = {QuotationMark = "`"}
            };

            return sqlTable;
        }

        public override IDbTable BuildTable(EntityInfo entityInfo)
        {
            var sqlTable = base.BuildTable(entityInfo);
            sqlTable.OutputOption.QuotationMark = "`";
            return sqlTable;
        }
    }
}