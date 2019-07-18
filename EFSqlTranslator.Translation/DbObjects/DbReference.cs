using System.Collections.Generic;
using System.Text;
using EFSqlTranslator.Translation.Extensions;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbReference : DbObject
    {
        public DbReference(IDbObject dbObject)
        {
            Referee = dbObject;

            OutputOption = dbObject?.OutputOption;
        }

        public IDbObject Referee { get; }

        // the select that contains the reference
        public IDbSelect OwnerSelect { get; set; }

        // the join that joining to this reference
        public IDbJoin OwnerJoin { get; set; }
        
        public IDictionary<string, IDbSelectable> RefSelection { get; set; } = new Dictionary<string, IDbSelectable>();

        public string Alias { get; set; }

        /// <summary>
        /// RefColumnAlias is used as alias in case we need to create a ref column for this dbRef
        /// </summary>
        public string RefColumnAlias { get; set; }

        /// <summary>
        /// The ref column which is referring to this reference
        /// all Select statement.
        /// </summary>
        public IDbRefColumn ReferredRefColumn { get; set; }

        public override string ToString()
        {
            if (Referee is IDbTable)
                return $"{Referee} {Alias}";

            var sb = new StringBuilder();

            sb.AppendLine("(");

            sb.AppendLineWithSpace(Referee.ToString());

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