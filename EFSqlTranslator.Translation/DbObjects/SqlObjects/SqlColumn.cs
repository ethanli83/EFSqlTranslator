using System;
using System.Text;
using EFSqlTranslator.Translation.Extensions;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlColumn : SqlSelectable, IDbColumn
    {
        public DbType ValType { get; set; }
        
        public string Name { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(Ref?.Alias))
                sb.Append($"{Ref.Alias}.");

            var qm = OutputOption.ForceQuotationMark || Name.Contains(" ") ? QuotationMark : string.Empty;
            sb.Append($"{qm}{Name}{qm}");

            return sb.ToString();
        }

        public override string ToSelectionString()
        {
            return !string.IsNullOrEmpty(Alias) && !Alias.Equals(Name, StringComparison.CurrentCulture)
                ? $"{this} as {QuotationMark}{Alias}{QuotationMark}"
                : $"{this}";
        }

        protected bool Equals(SqlColumn other)
        {
            return Equals(Ref, other.Ref) && Equals(ValType, other.ValType) && string.Equals(this.GetAliasOrName(), other.GetAliasOrName());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SqlColumn) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (ValType?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}