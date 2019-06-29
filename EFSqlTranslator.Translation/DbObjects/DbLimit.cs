namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbLimit : DbObject
    {
        public DbLimit(int offset, int fetch)
        {
            Offset = offset;
            Fetch = fetch;
        }

        public int Offset { get; set; }
        
        public int Fetch { get; set; }
    }
}