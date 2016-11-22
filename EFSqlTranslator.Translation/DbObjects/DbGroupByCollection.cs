using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbGroupByCollection : IDbObject, IEnumerable<IDbSelectable>
    {
        private List<IDbSelectable> _groupBys = new List<IDbSelectable>();

        public void Add(IDbSelectable selectable)
        {
            _groupBys.Add(selectable);
        }

        public bool IsSingleKey { get; set; }

        public T[] GetChildren<T>(Func<T, bool> filterFunc = null) where T : IDbObject
        {
            return _groupBys.SelectMany(s => s.GetChildren<T>(filterFunc)).ToArray();
        }

        public override string ToString()
        {
            var groupbys = from groupBy in _groupBys
                           let refCol = groupBy as IDbRefColumn
                           select refCol == null ? new [] { groupBy } : refCol.GetPrimaryKeys();
                           
            return string.Join(", ", groupbys.SelectMany(g => g));
        }

        public IEnumerator<IDbSelectable> GetEnumerator()
        {
            return _groupBys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _groupBys.GetEnumerator();
        }
    }
}