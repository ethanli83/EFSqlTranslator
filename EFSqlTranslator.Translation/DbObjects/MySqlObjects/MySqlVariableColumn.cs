namespace EFSqlTranslator.Translation.DbObjects.MySqlObjects
{
    public class MySqlVariableColumn : DbObject, IDbSelectable
    {
        private readonly string _script;

        public MySqlVariableColumn(string script)
        {
            _script = script;
        }

        public override string ToString()
        {
            return $"{_script} as '{Alias}'";
        }

        public DbReference Ref { get; set; }

        public IDbSelect OwnerSelect { get; set; }

        public string Alias { get; set; }

        public bool IsJoinKey { get; set; }

        public bool IsAggregation { get; set; }

        public string ToSelectionString()
        {
            return ToString();
        }
    }
}