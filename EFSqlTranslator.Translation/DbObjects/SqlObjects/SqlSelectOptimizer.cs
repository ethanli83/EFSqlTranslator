using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public static class SqlSelectOptimizer
    {
        public static IDbSelect UnwrapUnneededSelect(IDbSelect dbSelect)
        {
            while (true)
            {
                if (!CanUnwrapSelect(dbSelect))
                    return dbSelect;

                var unwrapedSelect = (IDbSelect) dbSelect.From.Referee;
                if (!dbSelect.Selection.Any())
                {
                    dbSelect = unwrapedSelect;
                    continue;
                }

                // re add columns with outer select's order
                var colDict = unwrapedSelect.Selection.ToDictionary(s => s.GetAliasOrName(), s => s);

                unwrapedSelect.Selection.Clear();
                foreach (var selectable in dbSelect.Selection)
                {
                    var innnerCol = colDict[selectable.GetNameOrAlias()];
                    innnerCol.Alias = selectable.GetAliasOrName();
                    unwrapedSelect.Selection.Add(innnerCol);
                }

                dbSelect = unwrapedSelect;
            }
        }

        private static bool CanUnwrapSelect(IDbSelect dbSelect)
        {
            return dbSelect.From.Referee is IDbSelect && dbSelect.Where == null &&
                   !dbSelect.Joins.Any() && !dbSelect.OrderBys.Any() && !dbSelect.GroupBys.Any() &&
                   dbSelect.Selection.All(s => s is IDbColumn || s is IDbRefColumn);
        }

        public static void RemoveUnneededSelectAllColumn(IDbSelect dbSelect, bool subSelectOnly = true)
        {
            if (!subSelectOnly)
            {
                var selectAllColumns = dbSelect.Selection.OfType<IDbRefColumn>().ToArray();
                foreach (var refColumn in selectAllColumns)
                    dbSelect.Selection.Remove(refColumn);
            }

            var subSelect = dbSelect.From?.Referee as IDbSelect;
            if (subSelect != null)
                RemoveUnneededSelectAllColumn(subSelect, false);

            foreach (var dbJoin in dbSelect.Joins)
            {
                subSelect = dbJoin.To.Referee as IDbSelect;
                if (subSelect != null)
                    RemoveUnneededSelectAllColumn(subSelect, false);
            }
        }
    }
}