namespace EFSqlTranslator.Translation.DbObjects.PostgresQlObjects
{
    public class PostgresQlLimit : DbObject, IDbLimit
    {
        public SqlLimit(int offset, int fetch)
        {
            Offset = offset;
            Fetch = fetch;
        }

        public int Offset { get; set; }
        
        public int Fetch { get; set; }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"offest {Offset} rows");
            sb.Append($"fetch next {Fetch} rows only");

            return sb.ToString();
        }
    }
}