using System;
using System.Collections.Generic;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbReference : IDbObject
    {
        public DbReference(IDbObject dbObject)
        {
            Referee = dbObject;
        }

        public IDbObject Referee { get; }

        // the select that contains the reference
        public IDbSelect OwnerSelect { get; set; }

        // the join that joining to this reference
        public IDbJoin OwnerJoin { get; set; }
        
        public IDictionary<string, IDbSelectable> RefSelection { get; set; } = new Dictionary<string, IDbSelectable>();

        //public IDbSelect OwnerSelect { get; set; }
        public string Alias { get; set; }

        public override string ToString()
        {
            if (Referee is IDbTable)
                return $"{Referee} {Alias}";

            var sb = new StringBuilder();

            sb.AppendLine("(");

            var refStr = Referee.ToString();
            var lines = refStr.Split(new [] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            refStr = string.Join("\n    ", lines);

            sb.AppendLine($"    {refStr}");
            sb.Append($") {Alias}");

            return sb.ToString();
        }

        protected bool Equals(DbReference other)
        {
            return Equals(Referee, other.Referee) && string.Equals(Alias, other.Alias) && Equals(OwnerSelect, other.OwnerSelect);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((DbReference) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Referee?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Alias?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (OwnerSelect?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}