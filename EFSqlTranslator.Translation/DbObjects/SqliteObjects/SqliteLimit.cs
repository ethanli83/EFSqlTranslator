using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqliteObjects
{
    public class SqliteLimit : DbLimit
    {
        public SqliteLimit(int offset, int fetch) : base(offset, fetch)
        {
        }
       
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"limit {Offset}, {Fetch}");
    
            return sb.ToString();
        }
    }
}