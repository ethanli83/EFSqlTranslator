using System;
using System.Text;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;

namespace EFSqlTranslator.Translation.DbObjects.SqliteObjects
{
    public class SqliteTempTable : SqlTempTable
    {
        public SqliteTempTable()
        {
            RowNumberColumnName = TranslationConstants.SqliteRowNumberColumnAlias;
        }

        public override IDbObject GetCreateStatement(IDbObjectFactory factory)
        {
            Func<string> action = () =>
            {
                var sb = new StringBuilder();

                sb.AppendLine($"create temporary table if not exists {this} as ");
                sb.AppendLineWithSpace(SourceSelect.ToString());
                sb.AppendLine();

                return sb.ToString();
            };

            return new DbDynamicStatement(action);
        }

        public override IDbObject GetDropStatement(IDbObjectFactory factory)
        {
            Func<string> action = () =>
            {
                var sb = new StringBuilder();
                sb.AppendLine($"drop table if exists {this}");
                return sb.ToString();
            };

            return new DbDynamicStatement(action);
        }
    }
}