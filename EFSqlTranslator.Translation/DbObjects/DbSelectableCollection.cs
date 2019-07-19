using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbSelectableCollection : DbObject, IEnumerable<IDbSelectable>
    {
        private readonly IDbSelect _owner;
        
        private readonly List<IDbSelectable> _selectables = new List<IDbSelectable>();

        public DbSelectableCollection(IDbSelect owner)
        {
            _owner = owner;
        }        

        public bool IsDistinct { get; set; }

        public void Add(IDbSelectable selectable)
        {
            if (_selectables.Contains(selectable))
                return;

            _selectables.Add(selectable);
            selectable.OwnerSelect = _owner;
            
            if (_owner.GroupBys.Any())
                _owner.GroupBys.Add(selectable);
        }

        public void Remove(IDbSelectable selectable)
        {
            if (!_selectables.Contains(selectable))
                return;

            _selectables.Remove(selectable);
        }

        public void Clear()
        {
            _selectables.Clear();
        }

        public override string ToString()
        {
            var selection = string.Empty;
            if (!_selectables.Any())
            {
                selection = $"{_owner.From.Alias}.*";
            }
            else 
            {
                var selections = 
                    from s in _selectables
                    select s.ToSelectionString();

                selection = string.Join(", ", selections);  
            } 

            return IsDistinct ? $"distinct {selection}" : selection;
        }

        public IEnumerator<IDbSelectable> GetEnumerator()
        {
            return _selectables.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _selectables).GetEnumerator();
        }
    }
}