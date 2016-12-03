using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlSelect : SqlObject, IDbSelect
    {
        public SqlSelect()
        {
            Selection = new DbSelectableCollection(this);
        }

        public DbSelectableCollection Selection { get; }
        
        public DbReference From { get; set; }
        
        public IDbObject Where { get; set; }

        public IList<IDbJoin> Joins { get; } = new List<IDbJoin>();

        public IList<IDbSelectable> OrderBys { get; } = new List<IDbSelectable>();
        
        public DbGroupByCollection GroupBys { get; } = new DbGroupByCollection();

        public bool IsWrapingSelect { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append($"select {Selection}");

            if (From != null)
            {
                sb.AppendLine();
                sb.Append($"from {From}");
            }

            if (Joins.Count > 0)
            {
                sb.AppendLine();
                sb.Append($"{string.Join(Environment.NewLine, Joins)}");
            }

            if (Where != null)
            {
                sb.AppendLine();
                sb.Append($"where {Where}");
            }

            if (GroupBys.Any())
            {
                sb.AppendLine();
                sb.Append($"group by {GroupBys}");
            }

            return sb.ToString();
        }

        public IDbSelect Optimize()
        {
            var dbSelect = Unwrap(this);
            return dbSelect;
        }

        private static IDbSelect Unwrap(IDbSelect dbSelect)
        {
            if (dbSelect.Selection.All(s => s is IDbColumn) && 
                dbSelect.From.Referee is IDbSelect &&
                dbSelect.Where == null && 
                !dbSelect.Joins.Any() && !dbSelect.OrderBys.Any() && !dbSelect.GroupBys.Any())
            {
                var unwrapedSelect = dbSelect.From.Referee as IDbSelect;
                if (!dbSelect.Selection.Any())
                    return Unwrap(unwrapedSelect);

                // re add columns with outer select's order
                var colDict = unwrapedSelect.Selection.
                    ToDictionary(s => s.GetAliasOrName(), s => s);

                unwrapedSelect.Selection.Clear();
                foreach(var column in dbSelect.Selection.Cast<IDbColumn>())
                {
                    var innnerCol = colDict[column.Name];
                    unwrapedSelect.Selection.Add(innnerCol);
                }

                return Unwrap(unwrapedSelect);
            }

            return dbSelect;
        }
    }
}