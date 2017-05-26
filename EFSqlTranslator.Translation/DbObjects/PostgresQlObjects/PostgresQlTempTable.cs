using EFSqlTranslator.Translation.DbObjects.SqliteObjects;

namespace EFSqlTranslator.Translation.DbObjects.PostgresQlObjects
{
    public class PostgresQlTempTable : SqliteTempTable
    {
        public PostgresQlTempTable()
        {
            RowNumberColumnName = TranslationConstants.PostgresQlRowNumberColumnAlias;
        }
    }
}