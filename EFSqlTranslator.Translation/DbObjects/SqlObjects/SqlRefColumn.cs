using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlRefColumn : SqlSelectable, IDbRefColumn
    {
        public IDbRefColumn RefTo { get; set; }

        public IDbColumn[] GetPrimaryKeysFromReferredQueryable()
        {
            var pks = RefTo?.GetPrimaryKeysFromReferredQueryable()?.ToArray() ??
                      (Ref.Referee as IDbTable)?.PrimaryKeys ?? new IDbColumn[0];

            foreach(var pk in pks)
                pk.Ref = Ref;

            return pks.ToArray();
        }

        public void AddToReferedSelect(string colName, Type colType, IDbObjectFactory factory, string alias = null)
        {
            AddToReferedSelect(colName, factory.BuildType(colType), factory, alias);
        }

        public void AddToReferedSelect(string colName, DbType colType, IDbObjectFactory factory, string alias = null)
        {
            if (RefTo != null)
            {
                RefTo.AddToReferedSelect(colName, colType, factory, alias);
                colName = alias ?? colName;
            }

            var column = factory.BuildColumn(Ref, colName, colType, alias);
            OwnerSelect.Selection.Add(column);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(Ref.Alias))
                sb.Append($"{Ref.Alias}.");

            sb.Append("*");

            return sb.ToString();
        }

        public override string ToSelectionString()
        {
            return ToString();
        }
    }
}