namespace Translation.DbObjects.SqlObjects
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
    }
}