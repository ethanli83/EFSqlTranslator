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
    }
}