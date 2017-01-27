using System.Collections.Generic;
using System.Linq;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

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
                // there are two ways to lookup the selectable in the unwraped select
                // for normally column, we can use the name the column to search aginst the
                // alais name in the unwrapped select
                // for RefColumn, we can use the RefTo property to look for the column it refers to
                var colDict = unwrapedSelect.Selection.
                    Where(s => s.GetAliasOrName() != null).
                    ToDictionary(s => s.GetAliasOrName(), s => s);

                unwrapedSelect.Selection.Clear();
                foreach (var selectable in dbSelect.Selection)
                {
                    var innnerCol = (selectable as IDbRefColumn)?.RefTo ?? colDict[selectable.GetNameOrAlias()];
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

        public static void MergeChildJoins(IDbSelect dbSelect)
        {
            var joins = dbSelect.Joins.Where(j => j.To.Referee is IDbSelect).ToArray();
            if (joins.Length == 0)
                return;

            foreach (var dbJoin in joins)
                MergeChildJoins((IDbSelect)dbJoin.To.Referee);

            var colDict = dbSelect.Selection.
                SelectMany(s => s.GetDbObjects<IDbColumn>()).
                Distinct().
                ToDictionary(s => s.GetAliasOrName(), s => s);

            var selectDict = new Dictionary<string, IDbJoin>();
            foreach (var dbJoin in joins)
            {
                var childSelect = (IDbSelect)dbJoin.To.Referee;

                var hashKey = childSelect.ToMergeKey();
                if (selectDict.ContainsKey(hashKey))
                {
                    var mergedJoin = selectDict[hashKey];
                    var mergedSelect = (IDbSelect)mergedJoin.To.Referee;

                    var colLookUp = mergedSelect.Selection.ToLookup(s => s.GetAliasOrName());
                    var columns = childSelect.Selection.Where(s => !colLookUp.Contains(s.GetAliasOrName()));
                    foreach (var selectable in columns)
                    {
                        var column = colDict[selectable.GetAliasOrName()];
                        column.Ref = mergedJoin.To;
                        mergedSelect.Selection.Add(selectable);
                    }
                }
                else
                {
                    selectDict[hashKey] = dbJoin;
                }
            }

            var lookUp = selectDict.Values.ToLookup(v => v);
            foreach (var dbJoin in joins)
            {
                if (!lookUp.Contains(dbJoin))
                    dbSelect.Joins.Remove(dbJoin);
            }
        }
    }
}