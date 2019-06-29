namespace EFSqlTranslator.Translation.DbObjects.MySqlObjects
{
    public class MySqlLimit : DbObject, IDbLimit
    {
        public MySqlLimit(int offset, int fetch)
        {
            Offset = offset;
            Fetch = fetch;
        }
    
        public int Offset { get; set; }
            
        public int Fetch { get; set; }
            
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"limit {Fetch} offset {Offset}");
    
            return sb.ToString();
        }
    }
}