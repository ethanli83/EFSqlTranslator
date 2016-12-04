using System.Collections.Generic;

namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbSelect : IDbObject
    {
        // columns or expression that the query return as result columns
        DbSelectableCollection Selection { get; }
        
        DbReference From { get; set; }
        
        IDbBinary Where { get; set; }

        IList<IDbJoin> Joins { get; }

        IList<IDbSelectable> OrderBys { get; }
        
        DbGroupByCollection GroupBys { get; }

        bool IsWrapingSelect { get; set; }

        IDbSelect Optimize();
    }
}