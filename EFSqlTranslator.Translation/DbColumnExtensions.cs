using System;
using System.Linq;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation
{
    public static class DbColumnExtensions
    {
        public static string GetAliasOrName(this IDbSelectable selectable)
        {
            var column = selectable as IDbColumn;
            return column != null ?  column.Alias ?? column.Name : selectable.Alias;
        }

        public static string GetNameOrAlias(this IDbSelectable selectable)
        {
            var column = selectable as IDbColumn;
            return column?.Name ?? selectable?.Alias;
        }

        /// <summary>
        /// Gets all primary keys from referenced tables. This function also
        /// recursively go throught all its RefTo ref columns. It is because the actual
        /// table that the ref column refering to maybe from sub selects.
        /// </summary>
        /// <returns></returns>
        public static IDbColumn[] GetPrimaryKeys(this IDbRefColumn refCol)
        {
            var pks = refCol.RefTo?.GetPrimaryKeys()?.ToArray() ??
                      (refCol.Ref.Referee as IDbTable)?.PrimaryKeys ?? new IDbColumn[0];

            foreach(var pk in pks)
                pk.Ref = refCol.Ref;

            return pks.ToArray();
        }

        /// <summary>
        /// Add selectable into the selection of the select which referred by the ref column.
        /// If the ref column has a RefTo ref column, this function will also recursively add
        /// the selectable to RefTo ref columns
        /// </summary>
        public static void AddToReferedSelect(
            this IDbRefColumn refCol, IDbObjectFactory factory, string colName, Type colType, string alias = null)
        {
            refCol.AddToReferedSelect(factory, colName, factory.BuildType(colType), alias);
        }

        /// <summary>
        /// Add selectable into the selection of the select which referred by the ref column.
        /// If the ref column has a RefTo ref column, this function will also recursively add
        /// the selectable to RefTo ref columns
        /// </summary>
        public static void AddToReferedSelect(
            this IDbRefColumn refCol, IDbObjectFactory factory, string colName, DbType colType, string alias = null)
        {
            if (refCol.RefTo != null)
            {
                refCol.RefTo.AddToReferedSelect(factory, colName, colType, alias);
                colName = alias ?? colName;
            }

            var column = factory.BuildColumn(refCol.Ref, colName, colType, alias);
            var selection = refCol.OwnerSelect.Selection;

            selection.Remove(refCol);
            selection.Add(column);
        }
    }
}