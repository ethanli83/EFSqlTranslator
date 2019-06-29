namespace EFSqlTranslator.Translation.DbObjects.SqliteObjects
{
    public class SqliteLimit : DbObject, IDbLimit
    {
        public SqliteLimit(int offset, int fetch)
        {
            Offset = offset;
            Fetch = fetch;
        }
    
        public int Offset { get; set; }
            
        public int Fetch { get; set; }
            
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"limit {Offset}, {Fetch}");
    
            return sb.ToString();
        }
    }
}