using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlRefColumn : SqlSelectable, IDbRefColumn
    {
        public IDbRefColumn RefTo { get; set; }

        public bool OnSelection { get; set; }

        public bool OnGroupBy { get; set; }

        public bool OnOrderBy { get; set; }

        public bool IsReferred { get; set; }

        public IDbColumn[] GetPrimaryKeys()
        {
            IDbColumn[] pks;
            if (RefTo != null)
            {
                pks =  RefTo.GetPrimaryKeys().ToArray();
            }
            else
            {
                var dbTable = Ref.Referee as IDbTable;
                pks = dbTable != null
                    ? dbTable.PrimaryKeys.ToArray()
                    : new IDbColumn[0];
            }

            foreach(var pk in pks)
                pk.Ref = Ref;

            return pks;
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
                sb.Append("*");
            }

            return sb.ToString();
        }

        public IList<IDbSelectable> GetRefSelection()
        {
            return Ref.RefSelection.Values.Select(c => { c.Ref = Ref; return c; }).ToArray();
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
                sb.Append("*");
            }

            return sb.ToString();
        }
    }
}