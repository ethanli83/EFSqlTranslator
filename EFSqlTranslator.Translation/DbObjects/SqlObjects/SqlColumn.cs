using System;
using System.Linq;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlColumn : SqlSelectable, IDbColumn
    {
        public DbType ValType { get; set; }
        
        public string Name { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(Ref.Alias))
                sb.Append($"{Ref.Alias}.");

            sb.Append($"'{Name}'");

            return sb.ToString();
        }

        public override string ToSelectionString()
        {
            return !string.IsNullOrEmpty(Alias) ? $"{this} as '{Alias}'" : $"{this}";
        }

        protected bool Equals(SqlColumn other)
        {
            return string.Equals(Name, other.Name) && Equals(ValType, other.ValType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((SqlColumn) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name?.GetHashCode() ?? 0) * 397) ^ (ValType?.GetHashCode() ?? 0);
            }
        }
    }
}