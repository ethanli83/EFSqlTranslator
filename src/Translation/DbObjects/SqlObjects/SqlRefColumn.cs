using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translation.DbObjects.SqlObjects
{
    public class SqlRefColumn : SqlSelectable, IDbRefColumn
    {
        public IDbRefColumn RefTo { get; set; }

        public bool OnSelection { get; set; }

        public bool OnGroupBy { get; set; }

        public bool OnOrderBy { get; set; }

        public override T[] GetChildren<T>(Func<T, bool> filterFunc = null)
        {
            return base.GetChildren<T>(filterFunc).
                Concat(Ref.GetChildren<T>(filterFunc)).
                Concat(RefTo.GetChildren<T>(filterFunc)).
                ToArray();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            var refToCols = GetRefSelection();
            if (refToCols.Any())
            {
                sb.Append(string.Join(", ", refToCols));
            }
            else
            {
                if (!string.IsNullOrEmpty(Ref.Alias))
                    sb.Append($"{Ref.Alias}.");
                sb.Append($"*");
            }

            return sb.ToString();
        }

        public IEnumerable<IDbSelectable> GetRefSelection()
        {
            return Ref.RefSelection.Values.Select(c => { c.Ref = Ref; return c; });
        }

        public override string ToSelectionString()
        {
            var sb = new StringBuilder();
            
            var refToCols = GetRefSelection();
            if (refToCols.Any())
            {
                var selection = refToCols.Where(r => !r.IsJoinKey).Select(v => v.ToSelectionString());
                sb.Append(string.Join(", ", selection));
            }
            else
            {
                if (!string.IsNullOrEmpty(Ref.Alias))
                    sb.Append($"{Ref.Alias}.");
                sb.Append($"*");
            }

            return sb.ToString();
        }
    }
}