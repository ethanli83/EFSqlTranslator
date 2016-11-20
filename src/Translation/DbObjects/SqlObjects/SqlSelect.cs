using System;
using System.Collections.Generic;
using System.Text;

namespace Translation.DbObjects.SqlObjects
{
    public class SqlSelect : SqlObject, IDbSelect
    {
        public SqlSelect()
        {
            Selection = new DbSelectableCollection(this);
        }

        public DbSelectableCollection Selection { get; private set; }
        
        public DbReference From { get; set; }
        
        public IDbObject Where { get; set; }

        public IList<IDbJoin> Joins { get; private set; } = new List<IDbJoin>();

        public IList<IDbSelectable> OrderBys { get; private set; } = new List<IDbSelectable>();
        
        public DbGroupByCollection GroupBys { get; private set; } = new DbGroupByCollection();

        public override T[] GetChildren<T>(Func<T, bool> filterFunc = null)
        {
            return base.GetChildren<T>(filterFunc);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("select " + Selection.ToString());

            if (From != null)
            {
                sb.AppendLine();
                sb.Append($"from {From}");
            }

            if (Joins.Count > 0)
            {
                sb.AppendLine();
                sb.Append($"{string.Join(Environment.NewLine, Joins)}");
            }

            if (Where != null)
            {
                sb.AppendLine();
                sb.Append($"where {Where}");
            }

            if (GroupBys != null && GroupBys.Any())
            {
                sb.AppendLine();
                sb.Append($"group by {string.Join(", ", GroupBys)}");
            }

            return sb.ToString();
        }
    }
}