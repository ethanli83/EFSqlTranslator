using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Translation.DbObjects
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
            _selectables.Add(selectable);
            selectable.OwnerSelect = _owner;
            //dbSelect.GroupBys.Add(selectable);
        }

        public override string ToString()
        {
            return string.Join(", ", _selectables);
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