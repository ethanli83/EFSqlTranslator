using System;
using System.Text;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;
using EFSqlTranslator.Translation.Extensions;

namespace EFSqlTranslator.Translation.DbObjects.MySqlObjects
{
    public class MySqlTempTable : SqlTempTable
    {
        public MySqlTempTable()
        {
            RowNumberColumnName = TranslationConstants.MySqlRowNumberColumnAlias;
        }

        public override IDbObject GetCreateStatement(IDbObjectFactory factory)
        {
            Func<string> action = () =>
            {
                var sb = new StringBuilder();
                sb.AppendLine($"create temporary table if not exists {this} as (");

                // add row
                var rowNumberScript = new MySqlVariableColumn("@var_row := @var_row + 1")
                {
                    Alias = TranslationConstants.MySqlRowNumberColumnAlias
                };

                SourceSelect.Selection.Add(rowNumberScript);

                var dbJoin = new MySqlVariableJoin("select @var_row := 0", "_var_row_");

                SourceSelect.Joins.Add(dbJoin);

                sb.AppendLineWithSpace(SourceSelect.ToString());

                sb.AppendLine(")");

                SourceSelect.Selection.Remove(rowNumberScript);
                SourceSelect.Joins.Remove(dbJoin);

                return sb.ToString();
            };

            return new DbDynamicStatement(action);
        }

        public override IDbObject GetDropStatement(IDbObjectFactory factory)
        {
            Func<string> action = () =>
            {
                var sb = new StringBuilder();
                sb.AppendLine($"drop temporary table {this}");
                return sb.ToString();
            };

            return new DbDynamicStatement(action);
        }
    }
}