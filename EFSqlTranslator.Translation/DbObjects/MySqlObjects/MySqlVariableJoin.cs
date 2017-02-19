namespace EFSqlTranslator.Translation.DbObjects.MySqlObjects
{
    public class MySqlVariableJoin : DbObject, IDbJoin
    {
        private readonly string _script;

        private readonly string _alias;

        public MySqlVariableJoin(string script, string alias)
        {
            _script = script;
            _alias = alias;
        }

        public override string ToString()
        {
            return $"inner join ({_script}) {_alias}";
        }

        public DbReference To { get; set; }

        public IDbBinary Condition { get; set; }

        public DbJoinType Type { get; set; }
    }
}