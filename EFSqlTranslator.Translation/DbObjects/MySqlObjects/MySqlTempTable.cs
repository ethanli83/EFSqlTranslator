using System;
using System.Text;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;

namespace EFSqlTranslator.Translation.DbObjects.MySqlObjects
{
    public class MySqlTempTable : SqlTempTable
    {
        public override IDbObject GetCreateStatement(IDbObjectFactory factory, UniqueNameGenerator nameGenerator)
        {
            Func<string> action = () =>
            {
                var sb = new StringBuilder();
                sb.AppendLine($"create temporary table if not exists {this} as (");

                // add row
                var alias = nameGenerator.GenerateAlias(SourceSelect, TranslationConstants.RowNumberColumnAlias, true);
                var rowNumberScript = new MySqlVariableColumn("@row := @row + 1")
                {
                    Alias = alias
                };

                SourceSelect.Selection.Add(rowNumberScript);

                alias = nameGenerator.GenerateAlias(SourceSelect, "row");
                var dbJoin = new MySqlVariableJoin("select @row := 0", alias);

                SourceSelect.Joins.Add(dbJoin);

                sb.AppendLineWithSpace(SourceSelect.ToString());

                sb.AppendLine(");");

                SourceSelect.Selection.Remove(rowNumberScript);
                SourceSelect.Joins.Remove(dbJoin);

                return sb.ToString();
            };

            return new DbDynamicStatement(action);
        }

        public override IDbObject GetDropStatement(IDbObjectFactory factory, UniqueNameGenerator nameGenerator)
        {
            Func<string> action = () =>
            {
                var sb = new StringBuilder();
                sb.AppendLine($"drop temporary table {this};");
                return sb.ToString();
            };

            return new DbDynamicStatement(action);
        }
    }
}