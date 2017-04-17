using System.Collections.Generic;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlTable : SqlObject, IDbTable
    {
        public string Namespace { get; set; }

        public IList<IDbColumn> PrimaryKeys { get; set; } = new List<IDbColumn>();

        public string TableName { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(Namespace))
                sb.Append($"{Namespace}.");

            var qm = TableName.Contains(" ") ? QuotationMark : string.Empty;
            sb.Append($"{qm}{TableName}{qm}");
            
            return sb.ToString();
        }

        protected bool Equals(SqlTable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Namespace, other.Namespace) && string.Equals(TableName, other.TableName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((SqlTable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Namespace?.GetHashCode() ?? 0) * 397) ^ (TableName?.GetHashCode() ?? 0);
            }
        }
    }
}