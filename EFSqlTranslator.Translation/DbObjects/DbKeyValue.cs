namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbKeyValue : DbObject
    {
        public DbKeyValue(string key, IDbObject val)
        {
            Key = key;
            Value = val;
        }

        public string Key { get; private set; }

        public IDbObject Value { get; private set; }
    }
}