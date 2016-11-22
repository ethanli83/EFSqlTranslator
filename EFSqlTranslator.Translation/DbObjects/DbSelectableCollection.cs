using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbSelectableCollection : IDbObject, IEnumerable<IDbSelectable>
    {
        private readonly IDbSelect _owner;
        
        private readonly List<IDbSelectable> _selectables = new List<IDbSelectable>();

        public DbSelectableCollection(IDbSelect owner)
        {
            _owner = owner;
        }        

        public void Add(IDbSelectable selectable)
        {
            if (_selectables.Contains(selectable))
                return;

            _selectables.Add(selectable);
            selectable.OwnerSelect = _owner;
            
            if (_owner.GroupBys.Any() && !_owner.GroupBys.Contains(selectable))
                _owner.GroupBys.Add(selectable);
        }

        public void Clear()
        {
            _selectables.Clear();
        }

        public override string ToString()
        {
            if (!_selectables.Any())
                return $"{_owner.From.Alias}.*";

            var selection = from s in _selectables
                            let rc = s as IDbRefColumn
                            where rc == null || (!rc.IsReferred && !rc.OnSelection)
                            select s.ToSelectionString();

            return string.Join(", ", selection);
        }

        public T[] GetChildren<T>(Func<T, bool> filterFunc = null) where T : IDbObject
        {
            return _selectables.SelectMany(s => s.GetChildren<T>(filterFunc)).ToArray();
        }

        IEnumerator<IDbSelectable> IEnumerable<IDbSelectable>.GetEnumerator()
        {
            return _selectables.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _selectables.GetEnumerator();
        }
    }
}