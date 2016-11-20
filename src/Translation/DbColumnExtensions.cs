using System;
using Translation.DbObjects;

namespace Translation
{
    public static class DbColumnExtensions
    {
        public static void AddColumnToReferedSubSelect(
            this IDbRefColumn refColumn, string colName, Type colType, IDbObjectFactory factory, string alias = null)
        {
            refColumn.AddColumnToReferedSubSelect(colName, factory.BuildType(colType), factory, alias);    
        }

        public static void AddColumnToReferedSubSelect(
            this IDbRefColumn refColumn, string colName, DbType colType, IDbObjectFactory factory, string alias = null)
        {
            if (refColumn.RefTo != null)
            {
                refColumn.RefTo.AddColumnToReferedSubSelect(colName, colType, factory, alias);
                colName = alias ?? colName;
            }

            var column = factory.BuildColumn(refColumn.Ref, colName, colType, alias);
            refColumn.OwnerSelect.Selection.Add(column);

            // set it to true so that this ref column will not need to be output
            // in the final result. 
            refColumn.IsReferred = true;   
        }

        public static string GetAliasOrName(this IDbSelectable selectable)
        {
            var column = selectable as IDbColumn;
            return column != null ?  column.Alias ?? column.Name : selectable.Alias;
        }
    }
}