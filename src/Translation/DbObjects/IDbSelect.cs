using System;
using System.Collections.Generic;
using System.Linq;

namespace Translation.DbObjects
{
    public interface IDbSelect : IDbObject
    {
        // columns or expression that the query return as result columns
        DbSelectableCollection Selection { get; }
        
        DbReference From { get; set; }
        
        IDbObject Where { get; set; }

        IList<IDbJoin> Joins { get; }

        IList<IDbSelectable> OrderBys { get; }
        
        DbGroupByCollection GroupBys { get; }
    }

    public class DbGroupByCollection : IDbObject
    {
        public IDictionary<string, IDbSelectable> GroupBys { get; } = 
            new Dictionary<string, IDbSelectable>();

        public void Add(IDbSelectable selectable)
        {
            var alias = GetAlias(selectable);
            GroupBys.Add(alias, selectable);
        }

        private string GetAlias(IDbSelectable selectable)
        {
            var alias = selectable.Alias;
            if (string.IsNullOrEmpty(alias))
            {
                var dbColumn = selectable as IDbColumn;
                if (dbColumn != null)
                    alias = dbColumn.Name;
                else
                    throw new InvalidOperationException("{key} does not have alias");
            }
            return alias;
        }

        public bool IsSingleKey { get; set; }

        public bool Any()
        {
            return GroupBys.Any();
        }

        public bool Contains(IDbSelectable selectable)
        {
            var alias = GetAlias(selectable);
            return GroupBys.ContainsKey(alias);
        }

        public T[] GetChildren<T>(Func<T, bool> filterFunc = null) where T : IDbObject
        {
            return GroupBys.Values.SelectMany(s => s.GetChildren<T>(filterFunc)).ToArray();
        }

        public override string ToString()
        {
            return string.Join(", ", GroupBys.Values);
        }
    }
}