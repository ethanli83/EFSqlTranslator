namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlSelectable : SqlObject, IDbSelectable 
    {
        public IDbObject SelectExpression { get; set; }

        public DbReference Ref { get; set; }

        public IDbSelect OwnerSelect { get; set; }

        public string Alias { get; set; }

        public bool IsJoinKey { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Alias) 
                ? $"{SelectExpression}"
                : $"{SelectExpression} as '{Alias}'";
        }

        public virtual string ToSelectionString()
        {
            return ToString();
        }

        protected bool Equals(SqlSelectable other)
        {
            return string.Equals(Alias, other.Alias) &&
                   Equals(OwnerSelect, other.OwnerSelect) &&
                   Equals(Ref, other.Ref) &&
                   Equals(SelectExpression, other.SelectExpression) &&
                   IsJoinKey == other.IsJoinKey;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as SqlSelectable;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Alias?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (OwnerSelect?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Ref?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (SelectExpression?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ IsJoinKey.GetHashCode();
                return hashCode;
            }
        }
    }
}