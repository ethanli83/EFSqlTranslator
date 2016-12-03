using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbGroupByCollection : IDbObject, IEnumerable<IDbSelectable>
    {
        private readonly List<IDbSelectable> _groupBys = new List<IDbSelectable>();

        public void Add(IDbSelectable selectable)
        {
            if (_groupBys.Contains(selectable))
                return;

            _groupBys.Add(selectable);
        }

        public bool IsSingleKey { get; set; }

        public override string ToString()
        {
            var groupbys = _groupBys.SelectMany(g =>
                (g as IDbRefColumn)?.GetPrimaryKeysFromReferredQueryable()?.Cast<IDbSelectable>() ??
                new[] {g});
                           
            return string.Join(", ", groupbys);
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